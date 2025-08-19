using Frost.Models;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Frost.Views
{
    public sealed partial class PlaytimeWindow : Window
    {
        public Game CurrentGame { get; private set; }
        private Process? _process;
        private CancellationTokenSource _cts;
        private DateTime _startTime;
        private bool _isDebugMode;
        private AppWindow _appWindow;

        // Constructor for real game
        public PlaytimeWindow(Game game, Process process)
        {
            this.InitializeComponent();

            CurrentGame = game;
            RootGrid.DataContext = CurrentGame;
            _process = process;
            _isDebugMode = false;

            InitializeAppWindow();

            _cts = new CancellationTokenSource();
            _startTime = DateTime.Now;

            StartPlaytimeCounter(_cts.Token);
            WatchForExit();
        }

        // Constructor for debug/testing
        public PlaytimeWindow(Game fakeGame)
        {
            this.InitializeComponent();

            CurrentGame = fakeGame;
            RootGrid.DataContext = CurrentGame;
            _isDebugMode = true;

            InitializeAppWindow();

            _cts = new CancellationTokenSource();
            _startTime = DateTime.Now;

            StartPlaytimeCounter(_cts.Token);

            // Auto-close after 10 minutes in debug mode
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                    DispatcherQueue.TryEnqueue(() => this.Close());
                }
            });
        }

        private void InitializeAppWindow()
        {
            // Get HWND and AppWindow
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Resize
            _appWindow.Resize(new Windows.Graphics.SizeInt32(300, 150));

            // Extend content into title bar
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

            // Customize title bar appearance
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            _appWindow.TitleBar.BackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            _appWindow.Title = _isDebugMode ? "Debug Playtime" : CurrentGame.Name;

            AppWindow.SetTitleBarIcon("Assets/Frost.png");
        }

        private async void StartPlaytimeCounter(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var elapsed = DateTime.Now - _startTime;

                DispatcherQueue.TryEnqueue(() =>
                {
                    PlaytimeText.Text = elapsed.ToString(@"hh\:mm\:ss");
                });

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException) { break; }
            }
        }

        private void WatchForExit()
        {
            if (_process == null || _isDebugMode)
                return;

            _process.EnableRaisingEvents = true;
            _process.Exited += (s, e) =>
            {
                _cts.Cancel();
                var elapsed = DateTime.Now - _startTime;

                Debug.WriteLine($"Game {CurrentGame.Name} session lasted {elapsed}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    this.Close();
                });
            };
        }
    }
}
