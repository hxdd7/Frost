using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Frost.DialogContents
{
    public sealed partial class DeletePluginConfirmationDialogContent : Page
    {
        public DeletePluginConfirmationDialogContent()
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
