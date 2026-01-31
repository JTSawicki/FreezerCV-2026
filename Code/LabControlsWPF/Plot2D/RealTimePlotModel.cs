using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace LabControlsWPF.Plot2D
{
    public class RealTimePlotModel
    {
        /// <summary>
        /// Zmienna maksymalnej ilości punktów wykresu.
        /// Wartość musi być większa niż 0.
        /// </summary>
        public int MaxPointCount
        {
            get { return _maxPointCount; }
            set
            {
                if(value > 0)
                    _maxPointCount = value;
                else
                    _maxPointCount = 10;
            }
        }
        private int _maxPointCount;

        /// <summary>Flaga czy wykres jest zatrzymany</summary>
        public bool PausePlot { get; set; } = false;

        /// <summary>Model wykresu OxyPlot</summary>
        internal PlotModel OxyPlotModel { get; private set; }

        /// <summary>Słownik serii danych używany do dodawania punktów</summary>
        private Dictionary<int, Series> _series = new Dictionary<int, Series>();
        /// <summary>Bufor punktów wykorzystywany na potrzyby zatrzymania wykresu</summary>
        private Dictionary<int, List<Tuple<double, double>>> _newPointBuffer = new Dictionary<int, List<Tuple<double, double>>>();

        /// <summary>
        /// Konstruktor modelu wykresu czasu rzeczywistego.
        /// Tworzy pusty model bez punktów
        /// </summary>
        /// <param name="title">Tytuł wykresu</param>
        /// <param name="yLabel">Tytuł osi Y</param>
        /// <param name="series">Serie danych</param>
        /// /// <param name="maxPoinCount">Maksymalna ilość punktów na wykresie</param>
        public RealTimePlotModel(string title, string yLabel, List<SeriesInitData> series, int maxPoinCount = 200)
        {
            _maxPointCount = maxPoinCount;

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
            OxyPlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = yLabel
            });
            OxyPlotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "Czas",
                StringFormat = "dd-HH:mm:ss"
            });

            // Dodawanie serii danych
            foreach (SeriesInitData elem in series)
            {
                if(elem.SeriesType == SeriesType.Line)
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
                    _newPointBuffer.Add(elem.ID, new List<Tuple<double, double>>());
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
                    _newPointBuffer.Add(elem.ID, new List<Tuple<double, double>>());
                }
            }
        }

        /// <summary>
        /// Funkcja dodaje nowy punkt do serii danych.
        /// Jeżeli seria danych o podanym ID nie istniej brak działania
        /// </summary>
        /// <param name="seriesID">Identyfikator serii danych</param>
        /// <param name="time">Czas na osi X</param>
        /// <param name="value">Wartość na osi Y</param>
        public void PushNewPoint(int seriesID, DateTime time, double value)
        {
            if (!_series.ContainsKey(seriesID))
                return;

            _newPointBuffer[seriesID].Add(
                new Tuple<double, double>(
                    OxyPlot.Axes.DateTimeAxis.ToDouble(time),
                    value
                    ));
            FlushBuffers();
        }

        /// <summary>
        /// Jeżeli wykres nie jest zatrzymany funkcja dodaje do serii punkty z buforów.
        /// </summary>
        public void FlushBuffers()
        {
            if (PausePlot)
                return;

            foreach(int id in _series.Keys)
            {
                // Sprawdzenie czy nie ma nadmiaru punktów w buforze
                if(_newPointBuffer[id].Count > MaxPointCount)
                {
                    _newPointBuffer[id].RemoveRange(0, _newPointBuffer[id].Count-MaxPointCount);
                }
                // Wypisywanie punktów
                foreach(Tuple<double, double> point in _newPointBuffer[id])
                {
                    if (_series[id] is LineSeries line)
                    {
                        if(line.Points.Count == MaxPointCount)
                        {
                            line.Points.RemoveAt(0);
                        }
                        else if (line.Points.Count > MaxPointCount)
                        {
                            line.Points.RemoveRange(0, line.Points.Count - (MaxPointCount + 1));
                        }
                        line.Points.Add(
                            new DataPoint(
                                point.Item1,
                                point.Item2
                            ));
                    }
                    if (_series[id] is ScatterSeries scatter)
                    {
                        if (scatter.Points.Count > MaxPointCount)
                        {
                            scatter.Points.RemoveAt(0);
                        }
                        scatter.Points.Add(
                            new ScatterPoint(
                                point.Item1,
                                point.Item2
                            ));
                    }
                }
                // Czyszczenie bufora
                _newPointBuffer[id].Clear();
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
            foreach(int id in _series.Keys)
            {
                for(int i=0; i<10; i++)
                {
                    PushNewPoint(id, DateTime.Now + TimeSpan.FromMinutes(i), 2*id+i*i);
                }
            }
        }
    }
}
