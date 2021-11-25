using AutoRefreshRateChangerWorker.Helpers;
using System.Runtime.InteropServices;
using static AutoRefreshRateChangerWorker.Helpers.NativeMethods;

namespace AutoRefreshRateChangerWorker.Services
{
    public class RefreshRateService
    {
        public DisplayMode DisplayMode { get; set; } = new DisplayMode();

        private DEVMODE vDevMode;

        public void PopulateDeviceInfo()
        {
            vDevMode = new();
            vDevMode.dmSize = (short)Marshal.SizeOf(vDevMode);

            _ = NativeMethods.EnumDisplaySettings(null!, NativeMethods.ENUM_CURRENT_SETTINGS, ref vDevMode);

            DisplayMode.Frequency = vDevMode.dmDisplayFrequency;
            DisplayMode.Width = vDevMode.dmPelsWidth;
            DisplayMode.Height = vDevMode.dmPelsHeight;
        }

        public void SwitchTo144()
        {
            vDevMode.dmDisplayFrequency = 144;
            NativeMethods.ChangeDisplaySettings(vDevMode, 0);
            DisplayMode.Frequency = vDevMode.dmDisplayFrequency;
        }

        public void SwitchTo60()
        {
            vDevMode.dmDisplayFrequency = 60;
            NativeMethods.ChangeDisplaySettings(vDevMode, 0);
            DisplayMode.Frequency = vDevMode.dmDisplayFrequency;
        }
    }
}
