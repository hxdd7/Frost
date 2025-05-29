using Frost.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frost.DialogContents
{
    public sealed partial class AddGameDialogContent : Page
    {
        public string GameName => NameInput.Text;
        public string CompanyName => CompanyInput.Text;
        public string BackgroundImgUrl => BackgroundImgInput.Text;
        public string CoverImgUrl => CoverImgInput.Text;
        public string IconUrl => IconImgInput.Text;
        public string Tags => TagsInput.Text;
        public long InstallSizeBytes { get; set; }
        public string InstallSizeFormatted { get; set; }

        public AddGameDialogContent()
        {
            this.InitializeComponent();

            // Sync initial theme with App.MainWindow
            if (App.MainWindow.Content is FrameworkElement root)
            {
                this.RequestedTheme = root.ActualTheme;

                // Optional: Listen for theme changes dynamically
                root.ActualThemeChanged += (s, e) =>
                {
                    this.RequestedTheme = root.ActualTheme;
                };
            }
        }

        private async void BrowseExePath_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".exe");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ExePathTextBox.Text = file.Path;

                string folderPath = System.IO.Path.GetDirectoryName(file.Path);
                InstallSizeBytes = await Task.Run(() => GameSizeHelper.GetDirectorySize(folderPath));
                InstallSizeFormatted = GameSizeHelper.FormatSize(InstallSizeBytes);
            }
        }

        // Optional: If needed to update missing sizes for specific games in the dialog context
        public async Task UpdateMissingGameSizesAsync()
        {
            await GameSizeHelper.UpdateMissingGameSizesAsync();
        }
    }
}