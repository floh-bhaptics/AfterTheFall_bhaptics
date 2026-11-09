// BhapticsSDK2Wrapper.cs
// Original source: https://github.com/bhaptics/tact-csharp2/blob/7fda6079f85fd2ab474e77304a7f376b80642f4f/tact-csharp2/tact-csharp2/BhapticsSDK2Wrapper.cs

// As of writing this, tact-csharp2 has an out-of-date dll (v2.0.0) - the latest dll is in the tact-cpp2 repo (v2.8.2)
// - updated for v2.8.2 .dll from tact-cpp2 repo (https://github.com/bhaptics/tact-cpp2/blob/bfe4c02b5d39f20d64bb09f2dd5d6e82f4e17fa8/sdk/lib/x64/bhaptics_library.dll)
// - updates are based on bhaptics.h (https://github.com/bhaptics/tact-cpp2/blob/master/sdk/include/bhaptics/bhaptics.h)

using System.Runtime.InteropServices;
using System;

namespace tact_csharp2
{
    public class BhapticsSDK2Wrapper
    {
        private const string ModuleName = "bhaptics_library";

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr bHapticsGetVersion();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool registryAndInit(string sdkAPIKey, string workspaceId, string initData);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool registryAndInitHost(string sdkAPIKey, string workspaceId, string initData, string url);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool wsIsConnected();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wsClose();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void bHapticsShutdown();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retryInitialize(string sdkAPIKey, string workspaceId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int play(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playParam(string eventId, int requestId, float intensity, float duration, float angleX, float offsetY);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void playWithoutResult(string eventId, int requestId, float intensity, float duration, float angleX, float offsetY);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playWithStartTime(string eventId, int requestId, int startMillis, float intensity, float duration, float angleX, float offsetY);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playWithStartTimeToDevice(string eventId, int requestId, int startMillis, float intensity, float duration, float angleX, float offsetY, int deviceIndex);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playDot(int requestId, int position, int durationMillis, int[] motors, int size);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playDotToDevice(int requestId, int position, int durationMillis, int[] motors, int size, int deviceIndex);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playWaveform(int requestId, int position, int[] motorValues, int[] playTimeValues, int[] shapeValues, int repeatCount, int motorLen);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playWaveformDk3(int requestId, int position, int[] motorValues, int[] playTimeValues, int[] shapeValues, int frequency, int repeatCount, int motorLen);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playPath(int requestId, int position, int durationMillis, float[] xValues, float[] yValues, int[] intensityValues, int Len);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playPathToDevice(int requestId, int position, int durationMillis, float[] xValues, float[] yValues, int[] intensityValues, int Len, int deviceIndex);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playLoop(string eventId, int requestId, float intensity, float duration, float angleX, float offsetY, int interval, int maxCount);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int playLoopToDevice(string eventId, int requestId, float intensity, float duration, float angleX, float offsetY, int interval, int maxCount, int deviceIndex);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getEventTime(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void pauseEvent(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void resume(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool stop(int requestId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool stopByEventId(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool stopAll();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isbHapticsConnected(PositionType position);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isPlaying();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isPlayingByRequestId(int requestId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isPlayingByEventId(string eventId);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr bHapticsGetHapticMessage(string appKey, string workspaceId, int lastVersion, out int status);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr bHapticsGetHapticMappings(string appKey, string workspaceId, int lastVersion, out int status);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isPlayerRunning();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool isPlayerInstalled();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool launchPlayer([MarshalAs(UnmanagedType.I1)] bool tryLaunch);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getDeviceInfoJson();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getHapticMappingsJson();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ping(string address);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool pingAll();

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool swapPosition(string address);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool setDeviceVsm(string address, int vsm);
    }

    // PositionType enum source: https://docs.bhaptics.com/sdk/unity/references/enum
    public enum PositionType
    {
        Vest = 0,
        ForearmL = 1,
        ForearmR = 2,
        Head = 3,
        HandL = 4,
        HandR = 5,
        FootL = 6,
        FootR = 7,
        GloveL = 8,
        GloveR = 9
    }
}
