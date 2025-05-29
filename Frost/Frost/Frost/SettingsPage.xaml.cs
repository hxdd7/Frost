using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;

namespace Frost
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        // This method is called when the page is navigated to (or when coming back from another page)
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Load the saved theme and sound settings when navigating to the page
            LoadThemeSetting();
            LoadSoundSetting();
        }

        private void LoadThemeSetting()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string theme = localSettings.Values["AppTheme"] as string ?? "Use system setting";

            ThemeComboBox.SelectedIndex = theme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0
            };

            // Apply the theme when it's loaded
            switch (theme)
            {
                case "Light":
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Light;
                    break;
                case "Dark":
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Default;
                    break;
            }
        }

        private void LoadSoundSetting()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("AppSound"))
            {
                bool isSoundOn = localSettings.Values["AppSound"].ToString() == "On";
                SoundToggle.IsOn = isSoundOn; // Update the toggle based on saved state
                // Apply the sound setting immediately
                UpdateSoundSetting(isSoundOn);
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string selectedTheme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Use system setting";

            localSettings.Values["AppTheme"] = selectedTheme;

            switch (selectedTheme)
            {
                case "Light":
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Light;
                    break;
                case "Dark":
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    ((FrameworkElement)App.MainWindow.Content).RequestedTheme = ElementTheme.Default;
                    break;
            }
        }

        private void SoundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            var localSettings = ApplicationData.Current.LocalSettings;

            // Save the sound setting
            localSettings.Values["AppSound"] = toggle!.IsOn ? "On" : "Off";

            // Apply the sound setting immediately
            UpdateSoundSetting(toggle.IsOn);
        }

        private void UpdateSoundSetting(bool isSoundOn)
        {
            // Update the sound player state based on the toggle
            if (isSoundOn)
            {
                // Enable sound
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
            }
            else
            {
                // Disable sound
                ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
            }
        }
    }
}
