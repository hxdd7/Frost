using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Frost.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        // Example properties
        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged(nameof(CpuUsage));
            }
        }

        public string CpuModel { get; set; } = "Intel Core i7-10700K";

        private ulong _totalMemory = 16384; // e.g., in MB
        private ulong _usedMemory = 8192;

        public ulong TotalMemory
        {
            get => _totalMemory;
            set
            {
                _totalMemory = value;
                OnPropertyChanged(nameof(TotalMemory));
            }
        }

        public ulong UsedMemory
        {
            get => _usedMemory;
            set
            {
                _usedMemory = value;
                OnPropertyChanged(nameof(UsedMemory));
            }
        }

        public string MemoryDescription => $"Total Memory: {TotalMemory} MB";
        public string MemoryUsageText => $"Used: {UsedMemory} MB / {TotalMemory} MB";

        public ObservableCollection<DriveInfoModel> Drives { get; } = new ObservableCollection<DriveInfoModel>();
        public ObservableCollection<DriverInfoModel> Drivers { get; } = new ObservableCollection<DriverInfoModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class DriveInfoModel
    {
        public string Name { get; set; }
        public string DriveType { get; set; }
        public string FileSystem { get; set; }
        public string TotalSizeFormatted { get; set; }
        public string FreeSpaceFormatted { get; set; }
    }

    public class DriverInfoModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; }
        public string Status { get; set; }
    }
}
