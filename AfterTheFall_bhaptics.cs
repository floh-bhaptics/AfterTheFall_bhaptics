using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBhapticsTactsuit;
using Snowbreed.Client;
using UnityEngine;
using Il2CppSystem;
using Vertigo.PlayerBody;
using Vertigo.Snowbreed;
using Vertigo.Snowbreed.Client;
using static AfterTheFall_bhaptics.Plugin;
using Vertigo.ECS;
using Il2CppSystem.Collections;
using UnhollowerBaseLib;

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
            public static uint[] shotgunsIds = { 13 };

            public static void Postfix(Gun __instance)
            {
                if (!__instance.IsEquippedLocally)
                {
                    return;
                }
                Log.LogWarning("AMMO TYPE " + __instance.GunData.AmmoType);
                bool isRight = (__instance.MainHandSide == Vertigo.VR.EHandSide.Right);
                if (shotgunsIds.Contains(__instance.GunData.AmmoType))
                {
                    tactsuitVr.ShootRecoil("Shotgun", isRight);
                }
                else
                {
                    tactsuitVr.ShootRecoil("Pistol", isRight);
                }
                if (__instance.grabbedHands.Count == 2)
                {
                    tactsuitVr.PlaybackHaptics("RecoilHands" + (isRight ? "_L" : "_R"));
                    tactsuitVr.PlaybackHaptics("RecoilArms" + (isRight ? "_L" : "_R"));
                }
            }
        }

        [HarmonyPatch(typeof(SnowbreedPlayerHealthModule), "OnHit")]
        public class PlayerOnDamaged
        {
            // CHEAT GOD MODE
            public static bool Prefix(SnowbreedPlayerHealthModule __instance)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                // all AI as well => return false as well
                else { return false; }
            }
            public static void Postfix(SnowbreedPlayerHealthModule __instance, HitArgs args)
            {
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 flattenedHit = new Vector3(args.PlayerData.HitDirection.x, 0f, args.PlayerData.HitDirection.z);
                    //initial hit vector seems a bit off
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
                        tactsuitVr.StartHeartBeat();
                    }

                    //Downed, frozen
                    if (__instance.IsDead || __instance.IsDowned || __instance.isKilled)
                    {
                        tactsuitVr.PlaybackHaptics("Frozen");
                        tactsuitVr.StopHeartBeat();
                        //TODO SLOW heartbeat instead ?? Stop it when full dead
                    }
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
                }
            }
        }
        /*
        [HarmonyPatch(typeof(ClientGrabMeleeAttackModule), "StartAttack")]
        public class PlayerOnGrabbedByJuggernaut
        {
            public static void Postfix(ClientGrabMeleeAttackModule __instance)
            {
                Log.LogWarning(__instance.Entity.Name);
                Vertigo.ECS.Entity localPawn = LightweightDebug.GetLocalPawn();
                if (__instance.Entity.Name.Equals(localPawn.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    tactsuitVr.PlaybackHaptics("JuggernautGrab");
                }
            }
        }
        */
    }
}
