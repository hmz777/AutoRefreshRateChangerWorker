using AutoRefreshRateChangerWorker.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AutoRefreshRateChangerWorker.Services
{
    public class PowerManagementService
    {
        public PowerPlanLevel ActivePlan { get; set; } = PowerPlanLevel.Undefined;

        [AllowNull]
        private IEnumerable<string> HighPerformanceProfiles;

        private readonly IConfiguration configuration;

        public PowerManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;

            LoadHighProfiles();
            ActivePlan = GetPowerPlanLevel(GetActiveSchemeName());
        }

        private void LoadHighProfiles()
        {
            var highProfs = configuration.GetSection("PlanConfig:HighPerformance").Get<string[]>();

            if (highProfs == null || highProfs.Length == 0)
            {
                throw new Exception("High Performance profiles are not specified.");
            }

            HighPerformanceProfiles = highProfs;
        }

        public Guid GetActiveScheme()
        {
            IntPtr GuidPtr = IntPtr.Zero;

            var ErrorCode = NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out GuidPtr);

            if (ErrorCode != 0)
            {
                if (GuidPtr != IntPtr.Zero) { NativeMethods.LocalFree(GuidPtr); }

                throw new Exception($"GetActiveGuid() failed with code {ErrorCode}");
            }

            if (GuidPtr == IntPtr.Zero)
            {
                if (GuidPtr != IntPtr.Zero) { NativeMethods.LocalFree(GuidPtr); }

                throw new Exception("GetActiveScheme() returned null pointer for GUID");
            }

            Guid ActiveSchema = (Guid)Marshal.PtrToStructure(GuidPtr, typeof(Guid))!;

            return ActiveSchema;
        }

        public string GetPowerPlanName(Guid guid)
        {
            IntPtr BufferPointer = IntPtr.Zero;
            uint BufferSize = 0;

            var ErrorCode = NativeMethods.PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, BufferPointer, ref BufferSize);
            if (ErrorCode != 0)
            {
                if (BufferPointer != IntPtr.Zero) { Marshal.FreeHGlobal(BufferPointer); }

                throw new Exception($"GetPowerPlanName() failed when getting buffer size with code {ErrorCode}");
            }

            if (BufferSize <= 0) { return String.Empty; }
            BufferPointer = Marshal.AllocHGlobal((int)BufferSize);

            ErrorCode = NativeMethods.PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, BufferPointer, ref BufferSize);
            if (ErrorCode != 0)
            {
                if (BufferPointer != IntPtr.Zero) { Marshal.FreeHGlobal(BufferPointer); }

                throw new Exception($"GetPowerPlanName() failed when getting buffer pointer with code {ErrorCode}");
            }

            string Name = Marshal.PtrToStringUni(BufferPointer)!;

            return Name;
        }

        public string GetActiveSchemeName()
        {
            return GetPowerPlanName(GetActiveScheme());
        }

        public PowerPlanLevel GetPowerPlanLevel(string PlanName)
        {
            return HighPerformanceProfiles.Contains(PlanName) ? PowerPlanLevel.High : PowerPlanLevel.Low;
        }
    }
}
