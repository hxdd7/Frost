using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Frost.Helpers;
using Frost.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Frost.DialogContents;
using Windows.Graphics.Display;
using System.Runtime.InteropServices;
using System.Drawing;
using DrawingImage = System.Drawing.Image;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using Windows.UI.Core;
using Microsoft.UI.Windowing;

namespace Frost
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<Game> PinnedGames { get; set; } = new();
        private List<Game> allGames = new();
        private List<Game> filteredGames = new();
        private readonly string PinnedGamesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Frost", "pinned_games.json");

        public static MainWindow? Instance { get; private set; }

        private const int HOTKEY_ID = 9000;
        private const uint MOD_CTRL_ALT = 0x0003; // Control + Alt
        private const uint VK_S = 0x53; // Virtual Key 'S'
        private const int WM_HOTKEY = 0x0312;

        private Window? _trayMenuWindow;
        private bool _isTrayMenuOpen = false;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public Frame ContentFrame => this.contentFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;

            ApplySavedPreferences();

            this.Closed += async (sender, args) =>
            {
                await GameSessionManager.StopAllSessionsAsync();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                UnregisterHotKey(hwnd, HOTKEY_ID);
            };

            ExtendsContentIntoTitleBar = true;
            //AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            SetTitleBar(AppTitleBar);

            this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            // Commenting out the window message hook and hotkey registration temporarily to isolate the issue
            // RegisterGlobalHotKey();
            // InitializeMessageHook();

            contentFrame.Navigate(typeof(GamePage));
            nvSample.Header = "Library";

            LoadPinnedGames();
        }
        private void OpenFrost_Click(object sender, RoutedEventArgs e)
        {
            this.Activate();
            _trayMenuWindow?.Close();
            _trayMenuWindow = null;
            _isTrayMenuOpen = false;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _trayMenuWindow?.Close();
            _trayMenuWindow = null;
            _isTrayMenuOpen = false;
            Application.Current.Exit();
        }
        // Commenting out this method temporarily
        /*
        private void RegisterGlobalHotKey()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            RegisterHotKey(hwnd, HOTKEY_ID, MOD_CTRL_ALT, VK_S);
        }
        */

        // Commenting out this method temporarily
        /*
        private void InitializeMessageHook()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowMessageHook.Attach(hwnd, WndProc);
        }
        */

        public async Task CaptureAndSaveScreenshot()
        {
            try
            {
                Debug.WriteLine("Starting screenshot capture...");

                var screenWidth = (int)DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
                var screenHeight = (int)DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;

                using var bitmap = new Bitmap(screenWidth, screenHeight);
                using var g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

                var folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Frost", "Screenshots");

                // Ensure folder exists before saving
                Directory.CreateDirectory(folderPath);

                var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var fullPath = Path.Combine(folderPath, filename);

                bitmap.Save(fullPath, DrawingImageFormat.Png);
                Debug.WriteLine($"Screenshot saved at: {fullPath}");

                // Show the notification on the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await ShowScreenshotNotification("Screenshot saved successfully!", "Success");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Screenshot error: {ex.Message}");

                // Show the error notification on the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await ShowScreenshotNotification($"Screenshot error: {ex.Message}", "Error");
                });
            }
        }

        private async Task ShowScreenshotNotification(string message, string title)
        {
            ScreenshotStatusText.Text = message;
            ScreenshotNotificationDialog.Title = title;
            await ScreenshotNotificationDialog.ShowAsync();
        }

private void ApplySavedPreferences()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Apply theme setting
            string theme = localSettings.Values["AppTheme"] as string ?? "Use system setting";
            ((FrameworkElement)this.Content).RequestedTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            // Apply sound setting
            if (localSettings.Values.ContainsKey("AppSound"))
            {
                bool isSoundOn = localSettings.Values["AppSound"]?.ToString() == "On";

                ElementSoundPlayer.State = isSoundOn
                    ? ElementSoundPlayerState.On
                    : ElementSoundPlayerState.Off;
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
                sender.Header = "Settings";
                return;
            }

            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                string selectedTag = selectedItem.Tag?.ToString() ?? string.Empty;

                Type? pageType = selectedTag switch
                {
                    "HomePage" => typeof(HomePage),
                    "GamePage" => typeof(GamePage),
                    "PluginsPage" => typeof(PluginsPage),
                    "TrackingPage" => typeof(Views.TrackingPage),
                    "SystemInfoPage" => typeof(Pages.SystemInfoPage),
                    _ => null,
                };

                if (pageType != null && contentFrame.CurrentSourcePageType != pageType)
                {
                    contentFrame.Navigate(pageType);
                    sender.Header = selectedItem.Content?.ToString() ?? string.Empty;
                }
            }
        }

        private async void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is Game game)
            {
                // Get the XamlRoot of the current page
                var xamlRoot = this.Content as FrameworkElement;

                if (xamlRoot == null)
                {
                    throw new InvalidOperationException("Unable to find XamlRoot in MainWindow.");
                }

                // Open the confirmation dialog
                var confirmationDialog = new GameLaunchConfirmationDialogContent();
                confirmationDialog.DataContext = game;

                var contentDialog = new ContentDialog
                {
                    XamlRoot = xamlRoot.XamlRoot,  // Set the XamlRoot for the dialog
                    Title = "Confirm Game Launch",
                    Content = confirmationDialog,
                    PrimaryButtonText = "Launch",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    RequestedTheme = currentTheme
                };

                // Await the dialog to wait for user interaction
                var result = await contentDialog.ShowAsync();

                // Check if the user clicked the Launch button
                if (result == ContentDialogResult.Primary)
                {
                    // Proceed with launching the game
                    LaunchGame(game); // Your method to start the game
                }

                // Prevent the NavigationView from selecting the item and navigating
                sender.SelectedItem = null;  // Clear the selection to keep the user on the current page
            }
        }

        private async void GameSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (allGames.Count == 0)
                    allGames = await DatabaseHelper.GetGamesAsync();

                string query = sender.Text.ToLower();
                filteredGames = allGames
                    .Where(game =>
                        (game.Name?.ToLower().Contains(query) == true) ||
                        (game.Tags?.ToLower().Contains(query) == true))
                    .ToList();

                sender.ItemsSource = filteredGames.Select(g => g.Name).ToList();
            }
        }

        private void GameSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is string selectedName)
            {
                NavigateToGamePageWithFilter(selectedName);
            }
        }

        private void GameSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.QueryText))
            {
                NavigateToGamePageWithFilter(args.QueryText);
            }
        }

        private void NavigateToGamePageWithFilter(string filter)
        {
            contentFrame.Navigate(typeof(GamePage), filter);
            nvSample.SelectedItem = GamePage; // Optional: keep nav selection in sync
            nvSample.Header = "Library";
        }

        // Method to pin a game and add it to the navigation view
        public void PinGame(Game game)
        {
            if (!PinnedGames.Contains(game))
            {
                PinnedGames.Add(game);
                AddPinnedGameToNavigation(game);
            }
        }

        // Method to add the pinned game to the NavigationView
        private void AddPinnedGameToNavigation(Game game)
        {
            var pinnedGameItem = new NavigationViewItem
            {
                Content = game.Name,
                Tag = game,
                Icon = new SymbolIcon(Symbol.Setting)
            };

            pinnedGameItem.RightTapped += NavigationView_RightTapped;  // Attach the event handler here

            var navigationView = (Frame)Window.Current.Content;
            if (navigationView != null)
            {
                var nvSample = navigationView.FindName("nvSample") as NavigationView;
                if (nvSample != null)
                {
                    nvSample.MenuItems.Insert(0, pinnedGameItem);  // Insert at the top of the menu
                }
            }
        }

        private async void LoadPinnedGames()
        {
            await DatabaseHelper.InitializeDatabase(); // Ensure DB is ready
            var pinnedGames = await DatabaseHelper.GetPinnedGamesAsync(); // Fetch from DB
            PinnedGames.Clear();

            foreach (var game in pinnedGames.DistinctBy(g => g.Id))
            {
                PinnedGames.Add(game);
            }

            UpdatePinnedGamesInNavigationView(); // Update the UI
        }

        public void UpdatePinnedGamesInNavigationView()
        {
            var headerIndex = nvSample.MenuItems.IndexOf(
                nvSample.MenuItems
                    .OfType<NavigationViewItemHeader>()
                    .FirstOrDefault(h => h.Content?.ToString() == "Games"));

            if (headerIndex == -1)
                return;

            while (nvSample.MenuItems.Count > headerIndex + 1 &&
                   nvSample.MenuItems[headerIndex + 1] is NavigationViewItem item &&
                   item.Tag is Game)
            {
                nvSample.MenuItems.RemoveAt(headerIndex + 1);
            }

            foreach (var game in PinnedGames.DistinctBy(g => g.Id))
            {
                var image = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(game.CoverImgUrl)),
                    Width = 36,
                    Height = 36,
                    Stretch = Stretch.UniformToFill,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var bitmapIcon = new BitmapIcon
                {
                    UriSource = new Uri(game.CoverImgUrl),
                    ShowAsMonochrome = false
                };

                var pinnedItem = new NavigationViewItem
                {
                    Tag = game,
                    Content = game,
                    ContentTemplate = RootGrid.Resources["GameNavItemTemplate"] as DataTemplate,
                    Padding = new Microsoft.UI.Xaml.Thickness(0)
                };

                nvSample.MenuItems.Insert(++headerIndex, pinnedItem);
            }
        }

        private void NavigationView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Debug.WriteLine("Right-click triggered!"); // Confirm that the event is firing

            var element = e.OriginalSource as FrameworkElement;

            // Traverse the visual tree upwards to find the NavigationViewItem
            var tappedItem = element?.Parent as NavigationViewItem;

            // If the parent is not a NavigationViewItem, keep going up the tree
            if (tappedItem == null)
            {
                var parent = element?.Parent;
                while (parent != null)
                {
                    Debug.WriteLine($"Parent Type: {parent.GetType()}"); // Log each parent type
                    if (parent is NavigationViewItem)
                        break;
                    parent = (parent as FrameworkElement)?.Parent;
                }

                tappedItem = parent as NavigationViewItem;
            }

            if (tappedItem?.Tag is Game game)
            {
                // Retrieve the MenuFlyout from resources
                var menu = (MenuFlyout)nvSample.Resources["PinnedGameContextMenu"];
                if (menu != null)
                {
                    // Set the Game object as the Tag for each MenuFlyoutItem
                    foreach (var item in menu.Items.OfType<MenuFlyoutItem>())
                    {
                        item.Tag = game;
                    }

                    // Show the menu at the right-clicked position
                    menu.ShowAt(element, e.GetPosition(element));
                }
            }
        }

        private void LaunchPinnedGame_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is Game game)
            {
                LaunchGame(game); // Calling the method to launch the game
            }
        }

        private void UnpinPinnedGame_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is Game game)
            {
                PinnedGames.Remove(game); // Remove from the list
                SavePinnedGames();        // Save the updated pinned games
                UpdatePinnedGamesInNavigationView(); // Refresh the navigation view
            }
        }

        private async void LaunchGame(Game game)
        {
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            try
            {
                await GameSessionManager.StartSessionAsync(game);
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to launch game: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = currentTheme
                };
                await dialog.ShowAsync();
            }
        }

        private void SavePinnedGames()
        {
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            try
            {
                string pinnedGamesJson = JsonSerializer.Serialize(PinnedGames);
                File.WriteAllText(PinnedGamesFilePath, pinnedGamesJson);
            }
            catch (Exception ex)
            {
                // Replace MessageBox with ContentDialog in WinUI
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to launch game: {ex.Message}",
                    CloseButtonText = "OK",
                    RequestedTheme = currentTheme
                };
                dialog.ShowAsync();
            }
        }

        private void OpenDebugPlaytimeWindow()
        {
            var fakeGame = new Game
            {
                Name = "Debug Adventure",
                Company = "Test Studio",
                CoverImgUrl = "https://via.placeholder.com/150x200.png?text=Debug+Cover",
                BackgroundImgUrl = "https://via.placeholder.com/600x300.png?text=Debug+Background",
                IconUrl = "https://via.placeholder.com/64.png?text=DBG"
            };

            var debugWindow = new Frost.Views.PlaytimeWindow(fakeGame);
            debugWindow.Activate();
        }

    }
}
