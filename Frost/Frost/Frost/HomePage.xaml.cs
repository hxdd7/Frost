using Frost.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Frost
{
    public sealed partial class HomePage : Page
    {
        public HomePageViewModel ViewModel { get; } = new HomePageViewModel();

        public HomePage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;

            // Optional: Populate dummy data or call methods to fetch system info
            LoadSystemInfo();
        }

        private void LoadSystemInfo()
        {
            // Example: populate some drives info
            ViewModel.Drives.Add(new DriveInfoModel
            {
                Name = "C:",
                DriveType = "Fixed",
                FileSystem = "NTFS",
                TotalSizeFormatted = "512 GB",
                FreeSpaceFormatted = "200 GB"
            });
            ViewModel.Drives.Add(new DriveInfoModel
            {
                Name = "D:",
                DriveType = "Fixed",
                FileSystem = "NTFS",
                TotalSizeFormatted = "1 TB",
                FreeSpaceFormatted = "750 GB"
            });

            // Example: populate drivers info
            ViewModel.Drivers.Add(new DriverInfoModel
            {
                Name = "NVIDIA Graphics Driver",
                Version = "527.56",
                Provider = "NVIDIA",
                Status = "Running"
            });
            ViewModel.Drivers.Add(new DriverInfoModel
            {
                Name = "Audio Driver",
                Version = "10.0.19041",
                Provider = "Realtek",
                Status = "Running"
            });

            // Simulate CPU and Memory usage
            ViewModel.CpuUsage = 42;
            ViewModel.TotalMemory = 16384;
            ViewModel.UsedMemory = 8200;
        }
    }
}
