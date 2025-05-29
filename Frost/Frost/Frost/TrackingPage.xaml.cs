using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Frost.Helpers;
using Frost.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Microsoft.UI.Xaml;
using OxyPlot.Legends;

namespace Frost.Views
{
    public sealed partial class TrackingPage : Page
    {
        public PlotModel Model { get; private set; }

        public TrackingPage()
        {
            this.InitializeComponent();
            this.Loaded += TrackingPage_Loaded;
        }

        private async void TrackingPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSessionsAndChartAsync();
        }

        private async Task LoadSessionsAndChartAsync()
        {
            var sessions = await DatabaseHelper.GetAllGameSessionsAsync();

            // Set ListView ItemsSource
            SessionListView.ItemsSource = sessions;

            // Group data by date
            var grouped = sessions
                .GroupBy(s => s.StartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalDurationHours = g.Sum(x => x.DurationSeconds) / 3600.0
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Prepare data points
            var dataPoints = grouped.Select((x, index) => new DataPoint(index, x.TotalDurationHours)).ToList();
            var labels = grouped.Select(x => x.Date.ToString("MM/dd")).ToList();

            // Create PlotModel
            Model = new PlotModel { Title = "Play Time" };

            // Configure axes
            Model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "DateAxis",
                ItemsSource = labels
            });

            Model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Hours Played"
            });

            Model.TextColor = OxyColor.Parse("#FF000000"); // Black or use system text brush color
            Model.PlotAreaBorderColor = OxyColor.Parse("#1E1E1E"); // Lighter border for Fluent

            var lineSeries = new LineSeries
            {
                Color = OxyColor.Parse("#0078D7"), // Fluent Blue (SystemAccentColor)
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColor.Parse("#0078D7"),
                StrokeThickness = 2,
                LineStyle = LineStyle.Solid
            };

            Model.PlotMargins = new OxyThickness(40, 10, 20, 30);

            // Add line series
            var series = new LineSeries
            {
                Title = "Total Duration",
                ItemsSource = dataPoints,
                DataFieldX = "X",
                DataFieldY = "Y",
                MarkerType = MarkerType.Circle
            };

            var isDarkTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark;

            Model.TextColor = isDarkTheme ? OxyColors.White : OxyColors.Black;
            Model.PlotAreaBorderColor = isDarkTheme ? OxyColor.FromRgb(60, 60, 60) : OxyColors.LightGray;

            Model.Series.Add(series);

            // Bind the model to the PlotView
            ChartControl.Model = Model;
        }
    }
}
