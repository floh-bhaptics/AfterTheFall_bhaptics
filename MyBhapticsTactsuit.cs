using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using Bhaptics.Tact;
using Bhaptics;
using BepInEx;
using AfterTheFall_bhaptics;
using System.Runtime.InteropServices;

using System.Resources;
using System.Globalization;
using System.Collections;


namespace MyBhapticsTactsuit
{

    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        public static float glideIntensity = 0.8f;
        public static int glidePause = 300;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, String> FeedbackMap = new Dictionary<String, String>();

#pragma warning disable CS0618 // remove warning that the C# library is deprecated
        public HapticPlayer hapticPlayer;
#pragma warning restore CS0618 

        private static RotationOption defaultRotationOption = new RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                PlaybackHaptics("HeartBeat");
                Thread.Sleep(1000);
            }
        }

        public TactsuitVR()
        {

            LOG("Initializing suit");
            try
            {
#pragma warning disable CS0618 // remove warning that the C# library is deprecated
                hapticPlayer = new HapticPlayer("AfterTheFall_bhaptics", "AfterTheFall_bhaptics");
#pragma warning restore CS0618
                suitDisabled = false;
            }
            catch { LOG("Suit initialization failed!"); }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
        }

        public void LOG(string logStr)
        {
            Plugin.Log.LogMessage(logStr);
        }

        void RegisterInternally(string configPath)
        {
            LOG("Patterns folder not found: " + configPath);
            LOG("Using internal patterns");
            ResourceSet resourceSet = AfterTheFall_bhaptics.Properties.Resource1.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

            foreach (DictionaryEntry dict in resourceSet)
            {
                try
                {
                    hapticPlayer.RegisterTactFileStr(dict.Key.ToString(), dict.Value.ToString());
                    LOG("Pattern registered: " + dict.Key.ToString());
                    FeedbackMap.Add(dict.Key.ToString(), dict.Value.ToString());
                }
                catch (Exception e) { LOG(e.ToString()); continue; }

            }

            systemInitialized = true;
        }

        void RegisterAllTactFiles()
        {
            if (suitDisabled) { return; }
            
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string assemblyFile = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(assemblyFile);
            LOG("Assembly path: " + myPath);
            string configPath = Path.Combine(myPath, "bHaptics");
            // If the directory doesn't exist, use the internal Resource
            if (!Directory.Exists(configPath)) { RegisterInternally(configPath); return; }
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    hapticPlayer.RegisterTactFileStr(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, tactFileStr);
            }
            
            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (suitDisabled) { return; }
            if (FeedbackMap.ContainsKey(key))
            {
                ScaleOption scaleOption = new ScaleOption(intensity, duration);
                hapticPlayer.SubmitRegisteredVestRotation(key, key, defaultRotationOption, scaleOption);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            if (suitDisabled) { return; }
            ScaleOption scaleOption = new ScaleOption(1f, 1f);
            RotationOption rotationOption = new RotationOption(xzAngle, yShift);
            hapticPlayer.SubmitRegisteredVestRotation(key, key, rotationOption, scaleOption);
        }

        public void Spell(bool isRightHand)
        {
            if (suitDisabled) { return; }
            // weaponName is a parameter that will go into the vest feedback pattern name
            // isRightHand is just which side the feedback is on
            // intensity should usually be between 0 and 1

            float duration = 1.0f;
            float intensity = 1.0f;
            var scaleOption = new ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            var rotationFront = new RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyHands = "SpellHand" + postfix;
            string keyArm = "SpellArm" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "SpellVest" + postfix;
            hapticPlayer.SubmitRegisteredVestRotation(keyHands, keyHands, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyArm, keyArm, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyVest, keyVest, rotationFront, scaleOption);
        }

        public void SwordRecoil(bool isRightHand, float intensity = 0.7f)
        {
            // Melee feedback pattern
            if (suitDisabled) { return; }
            float duration = 1.0f;
            var scaleOption = new ScaleOption(intensity, duration);
            var rotationFront = new RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyHand = "RecoilHands" + postfix;
            string keyArm = "RecoilArms" + postfix;
            string keyVest = "RecoilBladeVest" + postfix;
            hapticPlayer.SubmitRegisteredVestRotation(keyHand, keyHand, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyArm, keyArm, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyVest, keyVest, rotationFront, scaleOption);
        }

        public void ShootRecoil(string gunType, bool isRightHand, float intensity = 0.7f)
        {
            // Melee feedback pattern
            if (suitDisabled) { return; }
            float duration = 1.0f;
            var scaleOption = new ScaleOption(intensity, duration);
            var rotationFront = new RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyHand = "RecoilHands" + postfix;
            string keyArm = "RecoilArms" + postfix;
            string keyVest = "Recoil" + gunType + "Vest" + postfix;
            hapticPlayer.SubmitRegisteredVestRotation(keyHand, keyHand, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyArm, keyArm, rotationFront, scaleOption);
            hapticPlayer.SubmitRegisteredVestRotation(keyVest, keyVest, rotationFront, scaleOption);
        }


        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StopHapticFeedback(String effect)
        {
            hapticPlayer.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                hapticPlayer.TurnOff(key);
            }
        }

        public void StopThreads()
        {
            StopHeartBeat();
        }


    }
}
