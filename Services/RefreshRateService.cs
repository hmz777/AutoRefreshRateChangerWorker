using AutoRefreshRateChangerWorker.Helpers;
using System.Runtime.InteropServices;
using static AutoRefreshRateChangerWorker.Helpers.NativeMethods;

namespace AutoRefreshRateChangerWorker.Services
{
    public class RefreshRateService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<RefreshRateService> logger;

        private Dictionary<int, DisplayMode> SupportedDisplayModes { get; set; } = new Dictionary<int, DisplayMode>();

        public readonly int HighFrequency;
        public readonly int LowFrequency;

        private DEVMODE CurrentDevMode;

        public RefreshRateService(IConfiguration configuration, ILogger<RefreshRateService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            HighFrequency = this.configuration.GetSection("PlanConfig").GetValue<int>("HighFrequency");
            LowFrequency = this.configuration.GetSection("PlanConfig").GetValue<int>("LowFrequency");

            CurrentDevMode = new();
            CurrentDevMode.dmSize = (short)Marshal.SizeOf(CurrentDevMode);
        }

        public void PopulateDeviceInfo()
        {
            _ = NativeMethods.EnumDisplaySettings(null!, NativeMethods.ENUM_CURRENT_SETTINGS, ref CurrentDevMode);

            var vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(vDevMode);

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
            var res = NativeMethods.ChangeDisplaySettings(CurrentDevMode, 0);

            if (res != 0)
            {
                logger.LogError("Switching to {freq}HZ failed.", HighFrequency);
            }
        }

        public void SwitchToLowFrequency()
        {
            CurrentDevMode.dmDisplayFrequency = LowFrequency;
            var res = NativeMethods.ChangeDisplaySettings(CurrentDevMode, 0);

            if (res != 0)
            {
                logger.LogError("Switching to {freq}HZ failed.", LowFrequency);
            }
        }

        public int GetCurrentDisplayFrequency()
        {
            return CurrentDevMode.dmDisplayFrequency;
        }
    }
}
