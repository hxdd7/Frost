using Microsoft.UI.Xaml.Controls;

namespace Frost.Pages
{
    public sealed partial class SystemInfoPage : Page
    {
        public string CpuName { get; }
        public string CpuCores { get; }
        public string GpuName { get; }
        public string GpuVram { get; }
        public string Ram { get; }
        public string OSVersion { get; }

        public SystemInfoPage()
        {
            this.InitializeComponent();

            CpuName = SystemInfoHelper.GetCpuName();
            CpuCores = $"Cores: {SystemInfoHelper.GetCpuCores()}";

            var gpu = SystemInfoHelper.GetGpuInfo();
            GpuName = gpu.Name;
            GpuVram = $"VRAM: {gpu.VRAM}";

            Ram = SystemInfoHelper.GetTotalRam();
            OSVersion = SystemInfoHelper.GetOSVersion();
        }
    }
}
