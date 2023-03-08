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
using Vertigo.Snowbreed.Client;

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
        public class StartGliding
        {
            public static void Postfix(Gun __instance)
            {
                bool isRight = (__instance.MainHandSide == Vertigo.VR.EHandSide.Right);
                tactsuitVr.ShootRecoil(isRight);
            }
        }


    }
}
