using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Frost.DialogContents
{
    public sealed partial class DeleteConfirmationDialogContent : Page
    {
        public DeleteConfirmationDialogContent()
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
