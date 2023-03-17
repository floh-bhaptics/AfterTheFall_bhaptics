﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Bhaptics.SDK2;
using AfterTheFall_bhaptics;

using System.Resources;
using System.Globalization;
using System.Collections;


namespace MyBhapticsTactsuit
{

    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent ZombieGrab_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, String> FeedbackMap = new Dictionary<String, String>();
        public static int heartBeatRate = 1000;

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                PlaybackHaptics("HeartBeat");
                Thread.Sleep(heartBeatRate);
            }
        }
        public void ZombieGrabFunc()
        {
            while (true)
            {
                // Check if reset event is active
                ZombieGrab_mrse.WaitOne();
                PlaybackHaptics("JuggernautGrab");
                Thread.Sleep(2000);
            }
        }

        public TactsuitVR()
        {

            LOG("Initializing suit");
            // Default configuration exported in the portal, in case the PC is not online
            var config = System.Text.Encoding.UTF8.GetString(AfterTheFall_bhaptics.Properties.Resource1.config);
            // Initialize with appID, apiKey, and default value in case it is unreachable
            var res = BhapticsSDK2.Initialize("VDgsXkzvLPIfwIBOTAX7", "uVIGumoIkQjWCnMxniVz", config);
            // if it worked, enable the suit
            suitDisabled = res != 0;

            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread ZombieGrabThread = new Thread(ZombieGrabFunc);
            ZombieGrabThread.Start();
        }

        public void LOG(string logStr)
        {
            Plugin.Log.LogMessage(logStr);
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (suitDisabled) return;
            BhapticsSDK2.Play(key.ToLower(), intensity, duration, 0f, 0f);
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            if (suitDisabled) { return; }
            BhapticsSDK2.Play(key.ToLower(), 1f, 1f, xzAngle, yShift);
        }


        public void ShootRecoil(string gunType, bool isRightHand, float intensity = 0.7f)
        {
            // Melee feedback pattern
            if (suitDisabled) { return; }
            float duration = 1.0f;
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyHand = "RecoilHands" + postfix;
            string keyArm = "RecoilArms" + postfix;
            string keyVest = "Recoil" + gunType + "Vest" + postfix;
            BhapticsSDK2.Play(keyHand.ToLower(), intensity, duration, 0f, 0f);
            BhapticsSDK2.Play(keyArm.ToLower(), intensity, duration, 0f, 0f);
            BhapticsSDK2.Play(keyVest.ToLower(), intensity, duration, 0f, 0f);
        }

        public void StartZombieGrab()
        {
            ZombieGrab_mrse.Set();
        }

        public void StopZombieGrab()
        {
            ZombieGrab_mrse.Reset();
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
            BhapticsSDK2.Stop(effect.ToLower());
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            BhapticsSDK2.StopAll();
        }

        public void StopThreads()
        {
            StopHeartBeat();
            StopZombieGrab();
        }


    }
}
