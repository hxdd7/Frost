using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Frost.DialogContents
{
    public sealed partial class AddPluginDialogContent : Page
    {
        // Define properties for PluginName, Developer, IconUrl, and ExePath
        public string PluginName
        {
            get => NameInput.Text;
            set => NameInput.Text = value;
        }

        public string Developer
        {
            get => DeveloperInput.Text;
            set => DeveloperInput.Text = value;
        }

        public string IconUrl
        {
            get => IconImgInput.Text;
            set => IconImgInput.Text = value;
        }

        public string ExePath
        {
            get => ExePathTextBox.Text;
            set => ExePathTextBox.Text = value;
        }

        public AddPluginDialogContent()
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
            }
        }
    }
}
