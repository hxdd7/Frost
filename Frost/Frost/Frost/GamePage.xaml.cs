using Frost.DialogContents;
using Frost.Helpers;
using Frost.Models;
using Frost.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace Frost
{
    public sealed partial class GamePage : Page
    {
        public ObservableCollection<Game> Games { get; set; } = new();
        private List<Game> games = new();
        private List<Game> AllGames = new();
        private string CurrentFilterQuery { get; set; }

        public GamePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string filter && !string.IsNullOrWhiteSpace(filter))
            {
                CurrentFilterQuery = filter;
            }

            await InitializePage();
        }

        private async Task InitializePage()
        {
            await DatabaseHelper.InitializeDatabase();
            await GameSizeHelper.UpdateMissingGameSizesAsync();
            await LoadGamesAsync();

            // Default sorting
            SortGames("Alphabetically_AZ");
        }

        private async Task LoadGamesAsync()
        {
            var loadedGames = await DatabaseHelper.GetGamesAsync();

            if (!string.IsNullOrWhiteSpace(CurrentFilterQuery))
            {
                loadedGames = loadedGames
                    .Where(g => g.Name?.ToLower().Contains(CurrentFilterQuery.ToLower()) == true)
                    .ToList();
            }

            games = loadedGames;
            Games.Clear();

            foreach (var game in loadedGames)
            {
                game.IsSelected = false;
                Games.Add(game);
            }

            ContentGridView.ItemsSource = Games;

            PopulateGenreFilter();
            PopulateDriveFilterMenu();

            UpdateEmptyState();
        }

        public static Visibility ReverseVisibility(Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void OpenAddGameDialog_Click(object sender, RoutedEventArgs e)
        {
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Add New Game",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                Content = new AddGameDialogContent(), 
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = currentTheme
            };

            // Show dialog and get result
            var result = await dialog.ShowAsync();

            // If the user clicks 'Add' (Primary Button)
            if (result == ContentDialogResult.Primary)
            {
                var dialogContent = (AddGameDialogContent)dialog.Content;

                // Validate URL for image before proceeding
                if (!Uri.IsWellFormedUriString(dialogContent.BackgroundImgUrl, UriKind.Absolute))
                {
                    await ShowContentDialogAsync("Invalid Image URL", "Please provide a valid image URL.");
                    return;
                }

                // Create a new game object
                var newGame = new Game
                {
                    Name = dialogContent.GameName,
                    Company = dialogContent.CompanyName,
                    BackgroundImgUrl = dialogContent.BackgroundImgUrl,
                    CoverImgUrl = dialogContent.CoverImgUrl,
                    IconUrl = dialogContent.IconUrl,
                    ExePath = dialogContent.ExePathTextBox.Text,
                    Tags = dialogContent.Tags,

                    InstallSizeBytes = dialogContent.InstallSizeBytes,
                    InstallSizeFormatted = dialogContent.InstallSizeFormatted
                };

                // Add game to the database and refresh the game list
                await DatabaseHelper.AddGameAsync(newGame);
                await LoadGamesAsync();
            }
        }

        private async void DeleteSelectedGames_Click(object sender, RoutedEventArgs e)
        {
            var selectedGames = GetSelectedGames();
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            // Check if any games are selected for deletion
            if (!selectedGames.Any())
            {
                await ShowContentDialogAsync("No Games Selected", "Please select at least one game to delete.");
                return;
            }

            // Show confirmation dialog
            ContentDialog confirmDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Confirm Deletion",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                Content = new DeleteConfirmationDialogContent(),
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = currentTheme
            };

            var result = await confirmDialog.ShowAsync();

            // If the user clicks 'Delete' (Primary Button)
            if (result == ContentDialogResult.Primary)
            {
                // Delete each selected game
                foreach (var game in selectedGames)
                {
                    await DatabaseHelper.DeleteGameAsync(game);
                }

                // Refresh the game list after deletion
                await LoadGamesAsync();
            }
        }

        private async Task ShowContentDialogAsync(string title, string content)
        {
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = content,
                CloseButtonText = "Ok",
                RequestedTheme = currentTheme
            };

            await dialog.ShowAsync();
        }

        private async void AddToCategoryContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var selectedGame = (sender as MenuFlyoutItem)?.Tag as Game;
            if (selectedGame == null) return;

            var dialogContent = new AddToCategoryDialog(selectedGame.Id);

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Add Game to Categories",
                Content = dialogContent,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await dialogContent.SaveSelectedCategoriesAsync();
            }
        }

        private ObservableCollection<Game> GetSelectedGames()
        {
            return new ObservableCollection<Game>(Games.Where(game => game.IsSelected));
        }

        private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle selection change
        }

        private void GridView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            var tappedItem = e.OriginalSource as FrameworkElement;

            if (tappedItem != null && tappedItem.DataContext is Game selectedGame)
            {
                var menuFlyout = Resources["GameContextMenuFlyout"] as MenuFlyout;

                if (menuFlyout != null)
                {
                    // Set game context
                    foreach (var item in menuFlyout.Items)
                    {
                        if (item is MenuFlyoutItem flyoutItem)
                        {
                            flyoutItem.Tag = selectedGame;
                        }
                    }

                    // Get the Pin/Unpin items
                    var pinItem = menuFlyout.Items
                        .OfType<MenuFlyoutItem>()
                        .FirstOrDefault(i => i.Name == "PinMenuItem");

                    var unpinItem = menuFlyout.Items
                        .OfType<MenuFlyoutItem>()
                        .FirstOrDefault(i => i.Name == "UnpinMenuItem");

                    // Check pin state and show only one
                    bool isPinned = MainWindow.Instance.PinnedGames.Any(g => g.Id == selectedGame.Id);
                    if (pinItem != null) pinItem.Visibility = isPinned ? Visibility.Collapsed : Visibility.Visible;
                    if (unpinItem != null) unpinItem.Visibility = isPinned ? Visibility.Visible : Visibility.Collapsed;

                    // Show menu
                    menuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
                }
            }
        }

        private async void DeleteGameContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var selectedGame = (sender as MenuFlyoutItem)?.Tag as Game;
            var root = App.MainWindow.Content as FrameworkElement;
            var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

            // Confirm deletion of the selected game
            if (selectedGame != null)
            {
                ContentDialog confirmDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Confirm Deletion",
                    Content = $"Are you sure you want to delete '{selectedGame.Name}'?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    RequestedTheme = currentTheme
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    // Delete the game and refresh the list
                    await DatabaseHelper.DeleteGameAsync(selectedGame);
                    await LoadGamesAsync();
                }
            }
        }

        private async void EditGameContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var selectedGame = (sender as MenuFlyoutItem)?.Tag as Game;

            if (selectedGame != null)
            {
                var dialogContent = new EditGameDialogContent
                {
                    GameName = selectedGame.Name,
                    CompanyName = selectedGame.Company,
                    BackgroundImgUrl = selectedGame.BackgroundImgUrl,
                    IconUrl = selectedGame.IconUrl,
                    CoverImgUrl = selectedGame.CoverImgUrl,
                    Tags = selectedGame.Tags
                };

                var root = App.MainWindow.Content as FrameworkElement;
                var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

                // Show the Edit Game dialog
                var editDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Edit Game",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    Content = dialogContent,
                    DefaultButton = ContentDialogButton.Primary,
                    RequestedTheme = currentTheme
                };

                var result = await editDialog.ShowAsync();

                // If the user clicks 'Save'
                if (result == ContentDialogResult.Primary)
                {
                    // Validate the image URL
                    if (!Uri.IsWellFormedUriString(dialogContent.BackgroundImgUrl, UriKind.Absolute))
                    {
                        await ShowContentDialogAsync("Invalid Image URL", "Please provide a valid image URL.");
                        return;
                    }

                    // Update the game details
                    selectedGame.Name = dialogContent.GameName;
                    selectedGame.Company = dialogContent.CompanyName;
                    selectedGame.BackgroundImgUrl = dialogContent.BackgroundImgUrl;
                    selectedGame.CoverImgUrl = dialogContent.CoverImgUrl;
                    selectedGame.IconUrl = dialogContent.IconUrl;
                    selectedGame.Tags = dialogContent.Tags;

                    // Save the updated game to the database
                    await DatabaseHelper.UpdateGameAsync(selectedGame);
                    await LoadGamesAsync();
                }
            }
        }

        private void LaunchGameContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var selectedGame = (sender as MenuFlyoutItem)?.Tag as Game;

            if (selectedGame != null && !string.IsNullOrEmpty(selectedGame.ExePath))
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedGame.ExePath,
                        UseShellExecute = true
                    });

                    if (process != null)
                    {
                        var playtimeWindow = new PlaytimeWindow(selectedGame, process);
                        playtimeWindow.Activate();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to launch game: {ex.Message}");
                }
            }
        }

        // Sorting Logic
        private void SortMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                string sortingOption = menuItem.Tag?.ToString();
                SortGames(sortingOption);
            }
        }

        private void SortGames(string sortingOption)
        {
            // Sort games based on the selected option
            switch (sortingOption)
            {
                case "Alphabetically_AZ":
                    Games = new ObservableCollection<Game>(Games.OrderBy(g => g.Name));
                    break;
                case "Alphabetically_ZA":
                    Games = new ObservableCollection<Game>(Games.OrderByDescending(g => g.Name));
                    break;
                case "Newest":
                    Games = new ObservableCollection<Game>(Games.OrderByDescending(g => g.Id));
                    break;
                case "Oldest":
                    Games = new ObservableCollection<Game>(Games.OrderBy(g => g.Id));
                    break;
                case "Size_ASC":
                    Games = new ObservableCollection<Game>(Games.OrderBy(g => g.InstallSizeBytes));
                    break;
                case "Size_DESC":
                    Games = new ObservableCollection<Game>(Games.OrderByDescending(g => g.InstallSizeBytes));
                    break;
            }

            // Refresh the GridView
            ContentGridView.ItemsSource = Games;
            UpdateEmptyState();
        }

        private async void PinGameContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuFlyoutItem)?.Tag is Game game)
            {
                if (game.IsPinned) return;

                await DatabaseHelper.PinGameAsync(game);

                if (App.MainWindow is MainWindow mainWin)
                {
                    mainWin.PinnedGames.Add(game);
                    mainWin.UpdatePinnedGamesInNavigationView();
                }

                await ShowContentDialogAsync("Pinned", $"{game.Name} was pinned to the navigation.");
            }
        }

        private async void UnpinGameContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuFlyoutItem)?.Tag is Game game)
            {
                await DatabaseHelper.UnpinGameAsync(game);

                if (App.MainWindow is MainWindow mainWin)
                {
                    var pinned = mainWin.PinnedGames.FirstOrDefault(g => g.Id == game.Id);
                    if (pinned != null)
                    {
                        mainWin.PinnedGames.Remove(pinned);
                        mainWin.UpdatePinnedGamesInNavigationView();
                    }
                }

                await ShowContentDialogAsync("Unpinned", $"{game.Name} was removed from the navigation.");
            }
        }

        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                bool boolValue = (bool)value;
                bool invert = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;

                return (invert ? !boolValue : boolValue) ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }

        private void PopulateGenreFilterMenu()
        {
            GenreFilterFlyout.Items.Clear();

            // "All" option
            var allItem = new MenuFlyoutItem { Text = "All" };
            allItem.Click += (s, e) => ApplyGenreFilter(null);
            GenreFilterFlyout.Items.Add(allItem);

            // Get distinct tags
            var allTags = games
                .SelectMany(g => g.TagList)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .OrderBy(tag => tag);

            foreach (var tag in allTags)
            {
                var item = new MenuFlyoutItem { Text = tag };
                item.Click += (s, e) => ApplyGenreFilter(new List<string> { tag });
                GenreFilterFlyout.Items.Add(item);
            }
        }

        private void PopulateGenreFilter()
        {
            // Clear previous items
            GenreFilterFlyout.Items.Clear();

            // Get unique tags from all games
            var allTags = games
                .SelectMany(game => game.TagList)
                .Distinct()
                .OrderBy(tag => tag);

            foreach (var tag in allTags)
            {
                var item = new ToggleMenuFlyoutItem
                {
                    Text = tag
                };
                item.Click += GenreFilterItem_Click;
                GenreFilterFlyout.Items.Add(item);
            }
        }

        private void GenreFilterItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedTags = GenreFilterFlyout.Items
                .OfType<ToggleMenuFlyoutItem>()
                .Where(item => item.IsChecked)
                .Select(item => item.Text)
                .ToList();

            ApplyGenreFilter(selectedTags);
        }

        private void ApplyGenreFilter(List<string> selectedTags)
        {
            if (selectedTags == null || selectedTags.Count == 0)
            {
                ContentGridView.ItemsSource = games; // original list
                return;
            }

            var filteredGames = games
                .Where(game => game.TagList.Any(tag => selectedTags.Contains(tag)))
                .ToList();

            ContentGridView.ItemsSource = filteredGames;
            UpdateEmptyState();
        }

        private async void ContentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedGame = e.ClickedItem as Game;

            if (selectedGame != null)
            {
                var gameDetailsDialog = new GameDetailsDialog();
                gameDetailsDialog.SetGameDetails(selectedGame);
                var root = App.MainWindow.Content as FrameworkElement;
                var currentTheme = root?.ActualTheme ?? ElementTheme.Default;

                var dialog = new ContentDialog
                {
                    //Title = selectedGame.Name,
                    Content = gameDetailsDialog,
                    PrimaryButtonText = "Close",
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    RequestedTheme = currentTheme
                };

                try
                {
                    await dialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error showing dialog: {ex.Message}");
                }
            }
        }

        private void PopulateDriveFilterMenu()
        {
            DriveFilterFlyout.Items.Clear();

            // "All Drives" option
            var allDrivesItem = new ToggleMenuFlyoutItem
            {
                Text = "All Drives",
                Tag = "All",
                IsChecked = _selectedDriveFilter == "All"
            };
            allDrivesItem.Click += DriveFilterToggleItem_Click;
            DriveFilterFlyout.Items.Add(allDrivesItem);

            var driveRoots = Games
                .Select(g => {
                    try { return Path.GetPathRoot(g.ExePath); }
                    catch { return null; }
                })
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d);

            foreach (var root in driveRoots)
            {
                try
                {
                    var driveInfo = new DriveInfo(root);
                    string label = string.IsNullOrWhiteSpace(driveInfo.VolumeLabel) ? "Local Disk" : driveInfo.VolumeLabel;
                    string displayName = $"{label} ({root.TrimEnd('\\')})";

                    var item = new ToggleMenuFlyoutItem
                    {
                        Text = displayName,
                        Tag = root.TrimEnd('\\').ToUpper(),
                        IsChecked = _selectedDriveFilter == root.TrimEnd('\\').ToUpper()
                    };
                    item.Click += DriveFilterToggleItem_Click;
                    DriveFilterFlyout.Items.Add(item);
                }
                catch
                {
                    // Ignore inaccessible drives
                }
            }
        }

        private string _selectedDriveFilter = "All";
        private void DriveFilterToggleItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleMenuFlyoutItem selectedItem)
            {
                _selectedDriveFilter = selectedItem.Tag?.ToString() ?? "All";

                foreach (var item in DriveFilterFlyout.Items.OfType<ToggleMenuFlyoutItem>())
                {
                    item.IsChecked = false;
                }
                selectedItem.IsChecked = true;

                Games.Clear();

                IEnumerable<Game> filteredGames;

                if (_selectedDriveFilter == "All")
                {
                    filteredGames = games;
                }
                else
                {
                    filteredGames = games.Where(game =>
                    {
                        try
                        {
                            var driveLetter = Path.GetPathRoot(game.ExePath)?.TrimEnd('\\').ToUpper();
                            return driveLetter == _selectedDriveFilter;
                        }
                        catch
                        {
                            return false;
                        }
                    });
                }

                foreach (var game in filteredGames)
                {
                    Games.Add(game);
                }

                ContentGridView.ItemsSource = Games;
                UpdateEmptyState();
            }
        }


        private void UpdateEmptyState()
        {
            EmptyPlaceholder.Visibility =
                Games == null || Games.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}
