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
            public static void Postfix(Gun __instance)
            {
                if(!__instance.IsEquippedLocally)
                { 
                    return;
                }
                bool isRight = (__instance.MainHandSide == Vertigo.VR.EHandSide.Right);
                tactsuitVr.ShootRecoil("Pistol", isRight);
                if(__instance.grabbedHands.Count == 2)
                {
                    tactsuitVr.PlaybackHaptics("RecoilHands" + (isRight ? "_L" : "_R"));
                    tactsuitVr.PlaybackHaptics("RecoilArms" + (isRight ? "_L" : "_R"));
                }
            }
        }

        [HarmonyPatch(typeof(ClientSnowbreedPlayerHealthModule), "ApplyDamage")]
        public class PlayerOnDamaged
        {
            public static void Postfix(ClientSnowbreedPlayerHealthModule __instance, Il2CppSystem.Nullable<Vector3> hitOrigin)
            {
                if (hitOrigin != null)
                {
                    Vector3 flattenedHit = new Vector3(hitOrigin.Value.x, 0f, hitOrigin.Value.z);
                    Vector3 patternOrigin = new Vector3(0f, 0f, 1f);
                    float earlyhitAngle = Vector3.Angle(flattenedHit, patternOrigin);
                    Vector3 earlycrossProduct = Vector3.Cross(flattenedHit, patternOrigin);
                    if (earlycrossProduct.y > 0f) { earlyhitAngle *= -1f; }
                    float myRotation = earlyhitAngle; //- playerDir.y;
                    myRotation *= -1f;
                    if (myRotation < 0f) { myRotation = 360f + myRotation; }
                    tactsuitVr.PlayBackHit("Slash", myRotation, 0.0f);
                }
            }
        }

    }
}
