using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Frost
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();

//#if DEBUG
//            // Create a fake game for testing
//            var fakeGame = new Frost.Models.Game
//            {
//                Name = "Debug Adventure",
//                Company = "Test Studio",
//                CoverImgUrl = "https://via.placeholder.com/150x200.png?text=Debug+Cover",
//                BackgroundImgUrl = "https://via.placeholder.com/600x300.png?text=Debug+Background",
//                IconUrl = "https://via.placeholder.com/64.png?text=DBG"
//            };

//            // Launch the playtime test window
//            var debugWindow = new Frost.Views.PlaytimeWindow(fakeGame);
//            debugWindow.Activate();
//#endif
        }

    }
}
