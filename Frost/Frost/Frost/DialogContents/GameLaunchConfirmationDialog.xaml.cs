using Frost.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Frost.DialogContents
{
    public sealed partial class GameLaunchConfirmationDialogContent : Page
    {
        public string? GameName => (this.DataContext as Game)?.Name;

        public GameLaunchConfirmationDialogContent()
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
    }
}
