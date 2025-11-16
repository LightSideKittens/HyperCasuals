using UnityEngine;

namespace FunGames.Tools.Utils
{
    public class DeviceInfo
    {
        public string DeviceModel { get; }
        public string DeviceType { get; }
        public string DeviceName { get; }
        public int SystemMemorySize { get; }
        public int GraphicsMemorySize { get; }
        public int ProcessorCount { get; }
        public int ProcessorFrequency { get; }

        public DeviceInfo()
        {
            DeviceModel = SystemInfo.deviceModel;
            DeviceType = SystemInfo.deviceType.ToString();
            DeviceName = SystemInfo.deviceName;
            SystemMemorySize = SystemInfo.systemMemorySize;
            GraphicsMemorySize = SystemInfo.graphicsMemorySize;
            ProcessorCount = SystemInfo.processorCount;
            ProcessorFrequency = SystemInfo.processorFrequency;
        }

        public static bool IsTablet()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            return diagonalInches > 6.5f && aspectRatio < 1.6f;
        }
    }
}