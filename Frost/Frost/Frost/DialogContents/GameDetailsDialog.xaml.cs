using Frost.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frost.DialogContents
{
    public sealed partial class GameDetailsDialog : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _gameTags = new();
        public List<string> GameTags
        {
            get => _gameTags;
            set
            {
                _gameTags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameTags)));
            }
        }

        public Game SelectedGame { get; set; }

        public GameDetailsDialog()
        {
            this.InitializeComponent();
            this.DataContext = this;

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

        public void SetGameDetails(Game game)
        {
            SelectedGame = game;
            GameTitle.Text = game.Name;
            GameCompany.Text = game.Company;
            GameSize.Text = game.InstallSizeFormatted;

            if (Uri.IsWellFormedUriString(game.CoverImgUrl, UriKind.Absolute))
            {
                GameCoverImage.Source = new BitmapImage(new Uri(game.CoverImgUrl));
            }

            GameTags = game.TagList.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
        }

        //private void OpenLocation_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button && button.Tag is string exePath && !string.IsNullOrWhiteSpace(exePath))
        //    {
        //        try
        //        {
        //            string folderPath = Path.GetDirectoryName(exePath);
        //            if (Directory.Exists(folderPath))
        //            {
        //                Process.Start(new ProcessStartInfo
        //                {
        //                    FileName = "explorer.exe",
        //                    Arguments = $"\"{folderPath}\"",
        //                    UseShellExecute = true
        //                });
        //            }
        //            else
        //            {
        //                Debug.WriteLine("Folder path does not exist.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine($"Error opening folder: {ex.Message}");
        //        }
        //    }
        //}
    }
}
