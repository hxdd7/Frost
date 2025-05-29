using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Frost.DialogContents
{
    public sealed partial class EditGameDialogContent : Page
    {
        public string GameName
        {
            get => NameInput.Text;
            set => NameInput.Text = value;
        }

        public string CompanyName
        {
            get => CompanyInput.Text;
            set => CompanyInput.Text = value;
        }

        public string BackgroundImgUrl
        {
            get => BackgroundImgInput.Text;
            set => BackgroundImgInput.Text = value;
        }

        public string CoverImgUrl
        {
            get => CoverImgInput.Text;
            set => CoverImgInput.Text = value;
        }

        public string IconUrl
        {
            get => IconImgInput.Text;
            set => IconImgInput.Text = value;
        }

        public string Tags
        {
            get => TagsInput.Text;
            set => TagsInput.Text = value;
        }

        public EditGameDialogContent()
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
