using Frost.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Frost
{
    public sealed partial class HomePage : Page
    {
        public HomePageViewModel ViewModel { get; } = new HomePageViewModel();

        public HomePage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
