using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Frost.Helpers;
using Frost.Models;
using System.Diagnostics;
using System;
using Frost.DialogContents;

namespace Frost
{
    public sealed partial class PluginsPage : Page
    {
        private ObservableCollection<Plugin> Plugins { get; set; } = new ObservableCollection<Plugin>();

        public PluginsPage()
        {
            this.InitializeComponent();
            InitializePage();
        }

        private async void InitializePage()
        {
            await DatabaseHelper.InitializeDatabase();
            await LoadPluginsAsync();

            // Apply default sorting (Alphabetically A-Z)
            SortPlugins("Alphabetically_AZ");
        }

        private async Task LoadPluginsAsync()
        {
            var plugins = await DatabaseHelper.GetPluginsAsync();
            Plugins.Clear();
            foreach (var plugin in plugins)
            {
                plugin.IsSelected = false; // Ensure IsSelected is false on page load
                Plugins.Add(plugin);
            }

            // Update GridView binding
            ContentGridView.ItemsSource = Plugins;
        }

        private async void OpenAddPluginDialog_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Add New Plugin",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
            };

            var dialogContent = new AddPluginDialogContent();
            dialog.Content = dialogContent;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newPlugin = new Plugin
                {
                    Name = dialogContent.PluginName,
                    Developer = dialogContent.Developer,
                    IconUrl = dialogContent.IconUrl,
                    ExePath = dialogContent.ExePath
                };

                // Add newPlugin to your database or collection
                await DatabaseHelper.AddPluginAsync(newPlugin);
            }
        }

        private async void DeletePluginContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var selectedPlugin = menuFlyoutItem?.Tag as Plugin;

            if (selectedPlugin != null)
            {
                ContentDialog confirmDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Confirm Deletion",
                    Content = new DeletePluginConfirmationDialogContent(), // Use the new DeletePluginConfirmationDialogContent for plugin
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    // Proceed to delete the selected plugin
                    await DatabaseHelper.DeletePluginAsync(selectedPlugin);
                    await LoadPluginsAsync();
                }
            }
        }

        private async Task ShowContentDialogAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = content,
                CloseButtonText = "Ok"
            };

            await dialog.ShowAsync();
        }

        private ObservableCollection<Plugin> GetSelectedPlugins()
        {
            return new ObservableCollection<Plugin>(Plugins.Where(plugin => plugin.IsSelected));
        }

        private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection change if needed
        }

        private void ContentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Plugin selectedPlugin)
            {
                Debug.WriteLine($"Plugin clicked: {selectedPlugin.Name}");
            }
        }

        private void GridView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            var tappedItem = e.OriginalSource as FrameworkElement;

            if (tappedItem != null && tappedItem.DataContext is Plugin selectedPlugin)
            {
                var menuFlyout = Resources["PluginContextMenuFlyout"] as MenuFlyout;

                if (menuFlyout != null)
                {
                    foreach (var item in menuFlyout.Items)
                    {
                        if (item is MenuFlyoutItem flyoutItem)
                        {
                            flyoutItem.Tag = selectedPlugin;
                        }
                    }

                    menuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
                }
            }
        }
        private async void EditPluginContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var selectedPlugin = menuFlyoutItem?.Tag as Plugin;

            if (selectedPlugin != null)
            {
                var editDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Edit Plugin",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel"
                };

                var dialogContent = new EditPluginDialogContent
                {
                    PluginName = selectedPlugin.Name,
                    DeveloperName = selectedPlugin.Developer,
                    IconImgUrl = selectedPlugin.IconUrl,
                    ExePath = selectedPlugin.ExePath
                };

                editDialog.Content = dialogContent;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Update the plugin details
                    selectedPlugin.Name = dialogContent.PluginName;
                    selectedPlugin.Developer = dialogContent.DeveloperName;
                    selectedPlugin.IconUrl = dialogContent.IconImgUrl;
                    selectedPlugin.ExePath = dialogContent.ExePath;

                    await DatabaseHelper.UpdatePluginAsync(selectedPlugin);
                    await LoadPluginsAsync();
                }
            }
        }

        private void LaunchPluginContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var selectedPlugin = menuFlyoutItem?.Tag as Plugin;

            if (selectedPlugin != null && !string.IsNullOrEmpty(selectedPlugin.ExePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedPlugin.ExePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to launch plugin: {ex.Message}");
                }
            }
        }

        // Sorting Logic
        private void SortMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                string sortingOption = menuItem.Tag.ToString();
                SortPlugins(sortingOption);
            }
        }

        private void SortPlugins(string sortingOption)
        {
            switch (sortingOption)
            {
                case "Alphabetically_AZ":
                    Plugins = new ObservableCollection<Plugin>(Plugins.OrderBy(p => p.Name));
                    break;
                case "Alphabetically_ZA":
                    Plugins = new ObservableCollection<Plugin>(Plugins.OrderByDescending(p => p.Name));
                    break;
                case "Newest":
                    Plugins = new ObservableCollection<Plugin>(Plugins.OrderByDescending(p => p.Id));
                    break;
                case "Oldest":
                    Plugins = new ObservableCollection<Plugin>(Plugins.OrderBy(p => p.Id));
                    break;
            }
            ContentGridView.ItemsSource = Plugins;
        }
    }
}
