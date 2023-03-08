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

        [HarmonyPatch(typeof(Gun), "FireBullet", new Type[] { typeof(bool), typeof(bool) })]
        public class FireGun
        {
            public static void Postfix(Gun __instance)
            {
                bool isRight = (__instance.MainHandSide == Vertigo.VR.EHandSide.Right);
                tactsuitVr.ShootRecoil("Pistol", isRight);
            }
        }

        [HarmonyPatch(typeof(PlayerBodyHit), "Apply", new Type[] { typeof(PlayerBodyBehaviour), typeof(Quaternion), typeof(Vertigo.VertigoAnimation.IK.FBBIKJobData) })]
        public class PlayerHit
        {
            public static void Postfix(PlayerBodyHit __instance)
            {
                tactsuitVr.LOG("Direction: " + __instance.localHeadHitDirection.x.ToString() + " " + __instance.localHeadHitDirection.z.ToString());
                Vector3 hitPosition = __instance.localHeadHitDirection;
                Vector3 flattenedHit = new Vector3(hitPosition.x, 0f, hitPosition.z);
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
