using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Frost.Helpers;
using Frost.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;
using System;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Media;

namespace Frost.Views
{
    public sealed partial class TrackingPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ISeries[] _seriesCollection = Array.Empty<ISeries>();
        public ISeries[] SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                _seriesCollection = value;
                OnPropertyChanged(nameof(SeriesCollection));
            }
        }

        private List<ICartesianAxis> _xAxes = new List<ICartesianAxis>();
        public List<ICartesianAxis> XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;
                OnPropertyChanged(nameof(XAxes));
            }
        }

        private List<ICartesianAxis> _yAxes = new List<ICartesianAxis>();
        public List<ICartesianAxis> YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                OnPropertyChanged(nameof(YAxes));
            }
        }

        public TrackingPage()
        {
            this.InitializeComponent();

            // Initialize to avoid binding nulls
            SeriesCollection = Array.Empty<ISeries>();
            XAxes = new List<ICartesianAxis>();
            YAxes = new List<ICartesianAxis>();

            this.Loaded += TrackingPage_Loaded;
            this.ActualThemeChanged += TrackingPage_ThemeChanged;
        }
        private async void TrackingPage_ThemeChanged(FrameworkElement sender, object args)
        {
            await LoadSessionsAndChartAsync(); // refresh to reapply theme-based styling
        }

        private async void TrackingPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSessionsAndChartAsync();
        }

        private SKColor GetAccentTextFillColorPrimary()
        {
            var brush = (SolidColorBrush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
            var color = brush.Color; // Windows.UI.Color or Microsoft.UI.Color depending on your namespace

            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private SKColor GetThemeTextColor()
        {
            if (Application.Current.Resources.TryGetValue("TextFillColorPrimaryBrush", out var resource) &&
                resource is SolidColorBrush brush)
            {
                var c = brush.Color;
                return new SKColor(c.R, c.G, c.B, c.A);
            }

            return IsDarkTheme() ? SKColors.LightGray : SKColors.Black;
        }

        private bool IsDarkTheme()
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var background = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            return background.R < 128 && background.G < 128 && background.B < 128;
        }

        private SKColor GetThemeColor(SKColor lightColor, SKColor darkColor)
        {
            return IsDarkTheme() ? darkColor : lightColor;
        }

        private async Task LoadSessionsAndChartAsync()
        {
            var sessions = await Task.Run(() => DatabaseHelper.GetAllGameSessionsAsync());

            var grouped = sessions
                .GroupBy(s => s.StartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalDurationMinutes = g.Sum(x => x.DurationSeconds) / 60.0
                })
                .OrderBy(d => d.Date)
                .ToList();

            var values = grouped.Select(g => g.TotalDurationMinutes).ToArray();
            var labels = grouped.Select(g => g.Date.ToString("MM/dd")).ToList();

            // Now update UI on UI thread:
            DispatcherQueue.TryEnqueue(() =>
            {
                var accentColor = GetAccentTextFillColorPrimary();
                var axisTextColor = GetThemeTextColor();
                var isDarkTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark;
                var labelColor = GetThemeColor(SKColors.Black, SKColors.LightGray);
                var gridLineColor = GetThemeColor(new SKColor(200, 200, 200), new SKColor(60, 60, 60));

                SeriesCollection = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Fill = null,
                        Stroke = new SolidColorPaint(accentColor, 4),
                        GeometryStroke = new SolidColorPaint(accentColor, 4),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("#272727"))
                    }
                };

                XAxes = new List<ICartesianAxis>
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 0,
                        Name = "Date",
                        NamePaint = new SolidColorPaint(new SKColor(128, 128, 128, 255)),
                        LabelsPaint = new SolidColorPaint(new SKColor(128, 128, 128, 255))
                    }
                };

                YAxes = new List<ICartesianAxis>
                {
                    new Axis
                    {
                        Name = "Minutes Played",
                        NamePaint = new SolidColorPaint(new SKColor(128, 128, 128, 255)),
                        LabelsPaint = new SolidColorPaint(new SKColor(128, 128, 128, 255)),
                        SeparatorsPaint = new SolidColorPaint(new SKColor(128, 128, 128, 50), 1)
                    }
                };

                SessionListView.ItemsSource = sessions;
            });
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
