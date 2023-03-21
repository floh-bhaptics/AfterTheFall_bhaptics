using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBhapticsTactsuit;
using UnityEngine;
using Il2CppSystem;
using Vertigo.PlayerBody;
using Vertigo.Snowbreed;
using Vertigo.Snowbreed.Client;
using static AfterTheFall_bhaptics.Plugin;
using Vertigo.ECS;
using Il2CppSystem.Collections;
using UnhollowerBaseLib;
using Vertigo.VRShooter;
using Vertigo.VR;
using Unity.Entities;

namespace AfterTheFall_bhaptics
{
    [BepInPlugin("org.bepinex.plugins.AfterTheFall_bhaptics", "After The Fall bhaptics integration", "1.0.0")]
    public class Plugin : BepInEx.IL2CPP.BasePlugin
    {
        internal static new ManualLogSource Log;
        public static TactsuitVR tactsuitVr;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo("Plugin AfterTheFall_bhaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
            // patch all functions
            var harmony = new Harmony("bhaptics.patch.afterthefall");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Gun), "FireBullet", new System.Type[] { typeof(bool), typeof(bool) })]
        public class FireGun
        {
            public static uint[] shotgunsIds = { 13, 42 };

            public static void Postfix(Gun __instance)
            {
                if (!__instance.IsEquippedLocally)
                {
                    return;
                }
                //Log.LogWarning("AMMO TYPE " + __instance.GunData.AmmoType);
                bool isRight = (__instance.MainHandSide == Vertigo.VR.EHandSide.Right);
                bool dualWield = (__instance.grabbedHands.Count == 2);
                if (shotgunsIds.Contains(__instance.GunData.AmmoType))
                {
                    tactsuitVr.ShootRecoil("Shotgun", isRight, dualWield);
                }
                else
                {
                    tactsuitVr.ShootRecoil("Pistol", isRight, dualWield);
                }
            }
        }

        [HarmonyPatch(typeof(SnowbreedPlayerHealthModule), "OnHit")]
        public class PlayerOnDamaged
        {
            public static void Postfix(SnowbreedPlayerHealthModule __instance, HitArgs args)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 flattenedHit = new Vector3(args.PlayerData.HitDirection.x, 0f, args.PlayerData.HitDirection.z);
                    Vector3 patternOrigin = new Vector3(0f, 0f, -1f);
                    float earlyhitAngle = Vector3.Angle(flattenedHit, patternOrigin);
                    //Log.LogWarning("HIT " + args.PlayerData.HitDirection);
                    //Log.LogWarning("Early angle " + earlyhitAngle);
                    Vector3 earlycrossProduct = Vector3.Cross(flattenedHit, patternOrigin);
                    //Log.LogWarning("Early CROSS " + earlycrossProduct);
                    if (earlycrossProduct.y > 0f) { earlyhitAngle *= -1f; }
                    float myRotation = earlyhitAngle;
                    myRotation *= -1f;
                    if (myRotation < 0f) { myRotation = 360f + myRotation; }
                    //Log.LogWarning("Rotation " + myRotation);
                    tactsuitVr.PlayBackHit("Slash", myRotation, 0.0f);

                    //Low Health
                    if (__instance.Health < (__instance.MaxHealth * 25 / 100))
                    {
                        //start heartbeat lowhealth
                        TactsuitVR.heartBeatRate = 1000;
                        tactsuitVr.StartHeartBeat();
                    }

                    //Downed, frozen
                    if (__instance.IsDowned)
                    {
                        tactsuitVr.PlaybackHaptics("Frozen");
                        TactsuitVR.heartBeatRate = 4000;
                        tactsuitVr.StartHeartBeat();
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(ClientSessionGameSystem), "HandleOnSessionStateChangedEvent")]
        public class OnSessionStateChange
        {
            public static void Postfix(ClientSessionGameSystem __instance)
            {
                tactsuitVr.StopHeartBeat();
                tactsuitVr.StopAllHapticFeedback();
                if(__instance.SessionEndingType == SessionGameSystem.ESessionEndingType.Failed)
                {
                    tactsuitVr.PlaybackHaptics("Death");
                }
            }
        }
        
        [HarmonyPatch(typeof(ClientExplosionGameSystem), "SpawnExplosion",
            new System.Type[] { typeof(Vertigo.Snowbreed.Client.ExplosionTO), typeof(Vector3), typeof(Quaternion), typeof(uint)})]
        public class OnExplosionSpawn
        {
            public static int explosionDistance = 25;
            public static void Postfix(ClientExplosionGameSystem __instance, Vector3 position)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                Vertigo.VRShooter.PawnTransformModule module = localPawn.GetModule<Vertigo.VRShooter.PawnTransformModule>();
                float distance = Vector3.Distance(module.GroundPosition, position);

                if (module != null && distance < explosionDistance)
                {
                    float intensity = (explosionDistance - distance) * 1.5f / explosionDistance;
                    tactsuitVr.PlaybackHaptics("ExplosionBelly", intensity);
                    tactsuitVr.PlaybackHaptics("ExplosionFeet", intensity);
                    tactsuitVr.PlaybackHaptics("ExplosionHead", intensity);
                }
            }
        }

        [HarmonyPatch(typeof(ClientSnowbreedPlayerHealthModule), "ApplyHeal")]
        public class PlayerOnHeal
        {
            public static void Postfix(ClientSnowbreedPlayerHealthModule __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.PlaybackHaptics("Healing");
                    if (__instance.Health >= (__instance.MaxHealth * 25 / 100))
                    {
                        //stop heartbeat lowhealth
                        tactsuitVr.StopHeartBeat();
                    }
                    else
                    {
                        //start heartbeat lowhealth in case you healed from frozen state and not enough health
                        TactsuitVR.heartBeatRate = 1000;
                        tactsuitVr.StartHeartBeat();
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(ZombieGrabAttackView), "Start")]
        public class PlayerOnGrabbedByJuggernautStart
        {
            public static void Postfix(ZombieGrabAttackView __instance, IClientAttackableTarget target)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (target.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.StartZombieGrab();
                }
            }
        }

        [HarmonyPatch(typeof(ZombieGrabAttackView), "Stop")]
        public class PlayerOnGrabbedByJuggernautStop
        {
            public static void Postfix(ZombieGrabAttackView __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                try
                {
                    if (__instance.targetEntityModuleData.targetPawnTrackedTransform.Entity.Name.Equals(
                        localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        tactsuitVr.StopZombieGrab();
                    }
                } catch (System.Exception)
                {
                    tactsuitVr.StopZombieGrab();
                }
            }
        }

        [HarmonyPatch(typeof(MissileCombatDeviceLocalController), "StopUse")]
        public class OnCombatDeviceItemUse
        {
            public static void Postfix(MissileCombatDeviceLocalController __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Owner.identityModule.Entity.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase) && __instance.Owner.CanBeActivated)
                {
                    tactsuitVr.PlaybackHaptics("MissilesHands_" + (__instance.Owner.isEquippedOnLeftHand ? "L" : "R"));
                    tactsuitVr.PlaybackHaptics("MissilesArms_" + (__instance.Owner.isEquippedOnLeftHand ? "L" : "R"));
                }
            }
        }

        [HarmonyPatch(typeof(ShockwavePunchDeviceItem), "SpawnExplosion")]
        public class OnShockwavePunchDeviceItemUse
        {
            public static void Postfix(ShockwavePunchDeviceItem __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.identityModule.Entity.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.PlaybackHaptics("ShockwaveHands_" + (__instance.isEquippedOnLeftHand ? "L" : "R"));
                    tactsuitVr.PlaybackHaptics("ShockwaveArms_" + (__instance.isEquippedOnLeftHand ? "L" : "R"));
                }
            }
        }

        [HarmonyPatch(typeof(SawbladeDeviceItem), "StopUse")]
        public class OnSawbladeDeviceItemUse
        {
            public static void Postfix(SawbladeDeviceItem __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.identityModule.Entity.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase) && __instance.CanBeActivated)
                {
                    tactsuitVr.PlaybackHaptics("SawbladeHands_" + (__instance.isEquippedOnLeftHand ? "L" : "R"));
                    tactsuitVr.PlaybackHaptics("SawbladeArms_" + (__instance.isEquippedOnLeftHand ? "L" : "R"));
                }
            }
        }

        [HarmonyPatch(typeof(ZiplineAttachableTransform), "StartZiplining")]
        public class OnZipLineEnter
        {
            public static void Postfix(ZiplineAttachableTransform __instance, Vertigo.ECS.Entity pawn, EHandSide handSide)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (pawn.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    TactsuitVR.ziplineHand = (handSide == EHandSide.Right) ? "R" : "L";
                    tactsuitVr.StartZipline();
                }
            }
        }

        [HarmonyPatch(typeof(Zipline), "StopUse")]
        public class OnZipLineExit
        {
            public static void Postfix(Zipline __instance, Vertigo.ECS.Entity pawn)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (pawn.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.StopZipline();
                }
            }
        }
        /*
        [HarmonyPatch(typeof(BoosterSpeedBuffCommand), "ApplyBoost")]
        public class OnRageBoosterStart
        {
            public static void Postfix(BoosterSpeedBuffCommand __instance, Entity user)
            {
                Log.LogMessage("RAGE START");
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (user.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    TactsuitVR.heartBeatRate = 500;
                    tactsuitVr.StartHeartBeat();
                }
            }
        }

        [HarmonyPatch(typeof(BoosterSpeedBuffCommand), "StopBoost")]
        public class OnRageBoosterStop
        {
            public static void Postfix(BoosterSpeedBuffCommand __instance, Entity user)
            {
                Log.LogMessage("RAGE STOP");
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (user.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.StopHeartBeat();
                }
            }
        }
        */
        [HarmonyPatch(typeof(ClientPadlock), "HandleOnHandEnterDetectionVolumeEvent")]
        public class OnClientPadlock
        {
            public static void Postfix(ClientPadlock __instance, Vertigo.ECS.Entity entity, Vertigo.VR.EHandSide handSide)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (entity.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.PlaybackHaptics("PadlockHands_" + (handSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
                    tactsuitVr.PlaybackHaptics("PadlockArms_" + (handSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
                }
            }
        }

        [HarmonyPatch(typeof(PlayerAudioModule), "PlayFootstepLocalPlayer")]
        public class bhaptics_FootStep
        {
            private static bool rightFoot = false;

            [HarmonyPostfix]
            public static void Postfix(PlayerAudioModule __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(
                    localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.PlaybackHaptics("FootStep_" + (bhaptics_FootStep.rightFoot ? "R" : "L"));
                    bhaptics_FootStep.rightFoot = !bhaptics_FootStep.rightFoot;
                }
            }
        }

        [HarmonyPatch(typeof(Gun), "OnMagazineEjected")]
        public class bhaptics_EjectMagazine
        {
            [HarmonyPostfix]
            public static void Postfix(Gun __instance)
            {
                if (!__instance.IsEquippedLocally)
                {
                    return;
                }
                tactsuitVr.PlaybackHaptics("MagazineEjectedHands_" + (__instance.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
                tactsuitVr.PlaybackHaptics("MagazineEjectedArms_" + (__instance.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
            }
        }

        [HarmonyPatch(typeof(GunAmmoInserter), "HandleAmmoInsertedEvent")]
        public class bhaptics_Reloading
        {
            [HarmonyPostfix]
            public static void Postfix(GunAmmoInserter __instance)
            {
                if (!__instance.gun.IsEquippedLocally)
                {
                    return;
                }

                tactsuitVr.PlaybackHaptics("MagazineReloadingHands_" + (__instance.gun.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
                tactsuitVr.PlaybackHaptics("MagazineReloadingArms_" + (__instance.gun.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
            }
        }

        [HarmonyPatch(typeof(GunAmmoInserter), "HandleMagInserterHandleFullyInsertedEvent")]
        public class bhaptics_Reloaded
        {
            [HarmonyPostfix]
            public static void Postfix(GunAmmoInserter __instance)
            {
                if (!__instance.gun.IsEquippedLocally)
                {
                    return;
                }

                tactsuitVr.PlaybackHaptics("MagazineLoadedHands_" + (__instance.gun.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
                tactsuitVr.PlaybackHaptics("MagazineLoadedArms_" + (__instance.gun.MainHandSide == Vertigo.VR.EHandSide.Right ? "R" : "L"));
            }
        }
    }
}
