using AutoRefreshRateChangerWorker.Helpers;
using System.Runtime.InteropServices;
using static AutoRefreshRateChangerWorker.Helpers.NativeMethods;

namespace AutoRefreshRateChangerWorker.Services
{
    public class RefreshRateService
    {
        private readonly IConfiguration configuration;

        private Dictionary<int, DisplayMode> SupportedDisplayModes { get; set; } = new Dictionary<int, DisplayMode>();

        public readonly uint HighFrequency;
        public readonly uint LowFrequency;

        private DEVMODE CurrentDevMode;

        public RefreshRateService(IConfiguration configuration)
        {
            this.configuration = configuration;
            HighFrequency = this.configuration.GetSection("PlanConfig").GetValue<uint>("HighFrequency");
            LowFrequency = this.configuration.GetSection("PlanConfig").GetValue<uint>("LowFrequency");

            CurrentDevMode = new();
            CurrentDevMode.dmSize = (ushort)Marshal.SizeOf(CurrentDevMode);
        }

        public void PopulateDeviceInfo()
        {
            _ = NativeMethods.EnumDisplaySettings(null!, NativeMethods.ENUM_CURRENT_SETTINGS, ref CurrentDevMode);

            var vDevMode = new DEVMODE();
            vDevMode.dmSize = (ushort)Marshal.SizeOf(vDevMode);

            int i = 0;
            while (NativeMethods.EnumDisplaySettings(null!, i, ref vDevMode))
            {
                SupportedDisplayModes.Add(i, new DisplayMode
                {
                    Frequency = vDevMode.dmDisplayFrequency,
                    Width = vDevMode.dmPelsWidth,
                    Height = vDevMode.dmPelsHeight
                });

                i++;
            }

            if (SupportedDisplayModes.Count == 0)
            {
                throw new Exception("No supported display modes are found or something went wrong");
            }
        }

        public void SwitchToHighFrequency()
        {
            CurrentDevMode.dmDisplayFrequency = HighFrequency;
            var res = NativeMethods.ChangeDisplaySettings(ref CurrentDevMode, 0);

            if (res != 0)
            {
                throw new Exception($"Switching to {HighFrequency}HZ failed with error code {res}.");
            }
        }

        public void SwitchToLowFrequency()
        {
            CurrentDevMode.dmDisplayFrequency = LowFrequency;
            var res = NativeMethods.ChangeDisplaySettings(ref CurrentDevMode, 0);

            if (res != 0)
            {
                throw new Exception($"Switching to {LowFrequency}HZ failed with error code {res}.");
            }
        }

        public uint GetCurrentDisplayFrequency()
        {
            return CurrentDevMode.dmDisplayFrequency;
        }
    }
}
