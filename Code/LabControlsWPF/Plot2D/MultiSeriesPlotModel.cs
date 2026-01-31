using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace LabControlsWPF.Plot2D
{
    public class MultiSeriesPlotModel
    {
        /// <summary>Model wykresu OxyPlot</summary>
        internal PlotModel OxyPlotModel { get; private set; }

        /// <summary>Słownik serii danych używany do dodawania punktów</summary>
        private Dictionary<int, Series> _series = new Dictionary<int, Series>();

        /// <summary>
        /// Konstruktor modelu wykresu wieloliniowego.
        /// Tworzy pusty model bez punktów
        /// </summary>
        /// <param name="title">Tytuł wykresu</param>
        /// <param name="xLabel">Tytuł osi X</param>
        /// <param name="yLabel">Tytuł osi Y</param>
        /// <param name="series">Serie danych</param>
        public MultiSeriesPlotModel(string title, string xLabel, string yLabel, List<SeriesInitData> series,
            AxisType xAxis = AxisType.Linear, AxisType yAxis = AxisType.Linear)
        {
            OxyPlotModel = new PlotModel()
            {
                Title = title,
                Subtitle = "Odświeżono: " + DateTime.Now.ToString("MM-dd HH:mm:ss")
            };

            // Tworzenie legendy
            Legend Oxy_Legend = new Legend
            {
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendPosition = LegendPosition.BottomRight,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical
            };
            OxyPlotModel.Legends.Add(Oxy_Legend);

            // Tworzenie opisanych osi wykresu
            if(xAxis == AxisType.Linear)
                OxyPlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Bottom,
                    Title = xLabel
                });
            else
                OxyPlotModel.Axes.Add(new OxyPlot.Axes.LogarithmicAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Bottom,
                    Title = xLabel
                });

            if (yAxis == AxisType.Linear)
                OxyPlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Left,
                    Title = yLabel
                });
            else
                OxyPlotModel.Axes.Add(new OxyPlot.Axes.LogarithmicAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Left,
                    Title = yLabel
                });

            // Dodawanie serii danych
            foreach (SeriesInitData elem in series)
            {
                if (elem.SeriesType == SeriesType.Line)
                {
                    LineSeries oxyLine = new LineSeries
                    {
                        Title = elem.Title,
                        Color = elem.Color,
                        LineJoin = LineJoin.Bevel,
                        LineStyle = LineStyle.Solid
                    };
                    OxyPlotModel.Series.Add(oxyLine);
                    _series.Add(elem.ID, oxyLine);
                }
                if (elem.SeriesType == SeriesType.Scatter)
                {
                    ScatterSeries oxyScatter = new ScatterSeries
                    {
                        Title = elem.Title,
                        MarkerType = MarkerType.Circle,
                        MarkerFill = OxyColors.Transparent,
                        MarkerStroke = elem.Color,
                        MarkerStrokeThickness = 1,
                        TrackerFormatString = "{Tag}\n{1}:{2:0.0}\n{3}:{4:0.0}"
                    };
                    OxyPlotModel.Series.Add(oxyScatter);
                    _series.Add(elem.ID, oxyScatter);
                }
            }
        }

        /// <summary>
        /// Funkcja plotuje nowy zestaw danych
        /// </summary>
        /// <param name="seriesID">Identyfikator serii danych</param>
        /// <param name="points">Lista punktów (x,y)</param>
        public void PushSeriesData(int seriesID, List<Tuple<double, double>> points)
        {
            if (!_series.ContainsKey(seriesID))
                return;

            if (_series[seriesID] is LineSeries line)
            {
                line.Points.Clear();
                foreach (Tuple<double, double> point in points)
                    line.Points.Add(
                        new DataPoint(
                            point.Item1,
                            point.Item2
                        ));
            }
            if (_series[seriesID] is ScatterSeries scatter)
            {
                scatter.Points.Clear();
                foreach (Tuple<double, double> point in points)
                    scatter.Points.Add(
                        new ScatterPoint(
                            point.Item1,
                            point.Item2
                        ));
            }
            // Wywołanie eventu odświeżenia
            OxyPlotModel.Subtitle = "Odświeżono: " + DateTime.Now.ToString("MM-dd HH:mm:ss");
            OxyPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// Plotuje zestaw testowych punktów na wszystkich seriach danych
        /// </summary>
        public void PlotDumbData()
        {
            foreach (int id in _series.Keys)
            {
                List<Tuple<double, double>> points = new List<Tuple<double, double>>();
                for (int i = 0; i < 10; i++)
                {
                    points.Add(new Tuple<double, double>(i * 2, 2 * id + i * i));
                }
                PushSeriesData(id, points);
            }
        }
    }

    public enum AxisType
    {
        Linear,
        Logarytmic
    }
}
