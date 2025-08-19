using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Frost.Models;

namespace Frost
{
    public sealed partial class GameDetailsPage : Page
    {
        public Game Game { get; set; }

        public GameDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Game game)
            {
                Game = game;
                this.DataContext = this; // Needed for {x:Bind Game.XYZ}
            }
        }
    }
}
