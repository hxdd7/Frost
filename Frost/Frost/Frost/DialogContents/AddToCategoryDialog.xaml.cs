using Frost.Helpers;
using Frost.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frost.DialogContents
{
    public sealed partial class AddToCategoryDialog : Page
    {
        private readonly int _gameId;
        private List<Category> _categories;

        public double ScrollViewerHeight { get; set; }

        public AddToCategoryDialog(int gameId)
        {
            InitializeComponent();
            _gameId = gameId;
            Loaded += AddToCategoryDialog_Loaded;
        }

        private async void AddToCategoryDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();

            // Calculate desired height of ScrollViewer
            double itemHeight = 36; // approx height of CheckBox
            double maxItemsVisible = 6; // max visible items
            ScrollViewerHeight = _categories.Count * itemHeight;
            if (ScrollViewerHeight > maxItemsVisible * itemHeight)
                ScrollViewerHeight = maxItemsVisible * itemHeight;

            CategoryScrollViewer.Height = ScrollViewerHeight;

            BuildCategoryCheckboxes();
            AnimateCategoryCheckboxes();
        }

        private async Task LoadCategoriesAsync()
        {
            _categories = await DatabaseHelper.GetCategoriesAsync();

            foreach (var cat in _categories)
            {
                cat.IsSelected = await DatabaseHelper.IsGameInCategoryAsync(cat.Id, _gameId);
            }
        }

        private void BuildCategoryCheckboxes()
        {
            CategoryCheckBoxPanel.Children.Clear();

            foreach (var cat in _categories)
            {
                var cb = new CheckBox
                {
                    Content = cat.Name,
                    IsChecked = cat.IsSelected,
                    Tag = cat,
                    Opacity = 0,
                    RenderTransform = new TranslateTransform { Y = 30 } // slide in from bottom
                };
                cb.Checked += CategoryCheckBox_Changed;
                cb.Unchecked += CategoryCheckBox_Changed;
                CategoryCheckBoxPanel.Children.Add(cb);
            }
        }

        private void AnimateCategoryCheckboxes()
        {
            var easing = new CircleEase { EasingMode = EasingMode.EaseInOut }; // CircleEase with ease-in-out
            int index = 0;
            foreach (var child in CategoryCheckBoxPanel.Children)
            {
                if (child is CheckBox cb)
                {
                    var sb = new Storyboard();

                    // Opacity animation
                    var opacityAnim = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(350),
                        EasingFunction = easing,
                        BeginTime = TimeSpan.FromMilliseconds(index * 50)
                    };
                    Storyboard.SetTarget(opacityAnim, cb);
                    Storyboard.SetTargetProperty(opacityAnim, "Opacity");
                    sb.Children.Add(opacityAnim);

                    // Translate Y animation
                    var translateAnim = new DoubleAnimation
                    {
                        From = 30,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(350),
                        EasingFunction = easing,
                        BeginTime = TimeSpan.FromMilliseconds(index * 50)
                    };
                    Storyboard.SetTarget(translateAnim, cb);
                    Storyboard.SetTargetProperty(translateAnim, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                    sb.Children.Add(translateAnim);

                    sb.Begin();
                    index++;
                }
            }
        }

        private void CategoryCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is Category cat)
                cat.IsSelected = cb.IsChecked == true;
        }

        private async void NewCategoryTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string newName = NewCategoryTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newName))
                {
                    var newCat = await DatabaseHelper.GetOrCreateCategoryByNameAsync(newName);
                    NewCategoryTextBox.Text = string.Empty;
                    await LoadCategoriesAsync();

                    // recalc height
                    double itemHeight = 36;
                    double maxItemsVisible = 6;
                    ScrollViewerHeight = _categories.Count * itemHeight;
                    if (ScrollViewerHeight > maxItemsVisible * itemHeight)
                        ScrollViewerHeight = maxItemsVisible * itemHeight;
                    CategoryScrollViewer.Height = ScrollViewerHeight;

                    BuildCategoryCheckboxes();
                    AnimateCategoryCheckboxes();
                }
            }
        }

        public async Task SaveSelectedCategoriesAsync()
        {
            foreach (var cat in _categories)
            {
                bool inDb = await DatabaseHelper.IsGameInCategoryAsync(cat.Id, _gameId);
                if (cat.IsSelected && !inDb)
                    await DatabaseHelper.AddGameToCategoryAsync(cat.Id, _gameId);
                else if (!cat.IsSelected && inDb)
                    await DatabaseHelper.RemoveGameFromCategoryAsync(cat.Id, _gameId);
            }
        }
    }
}
