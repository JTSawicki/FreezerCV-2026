using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LabControlsWPF;
using LabControlsWPF.Plot2D;
using LabServices.GpibHardware;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezerGUI.ViewModels
{
    public partial class Measurement2DPlotVM : ObservableObject, IDisposable
    {
        public Measurement2DPlotVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów
            BootStrapper.GpibController.NewMeasuermentEvent += PlotNewMeasuermentEvent;

            // Inicjalizowanie komend
            ChangePlotParametersCommand = new RelayCommand(ChangePlotParameters);
            GenerateDataCommand = new RelayCommand(GenerateData);

            // Konstruowanie modelu wykresu
            plotModel = new MultiSeriesPlotModel("Erorr", "", "", new List<SeriesInitData>());
            GenerateNewPlotModel();
        }

        public void InitializeDefaultValues()
        {
            ;
        }

        // Pola
        // --------------------------------------------------

        [ObservableProperty]
        private MultiSeriesPlotModel plotModel;

        private const int minShownMeasurementCount = 1;
        private const int maxShownMeasurementCount = 20;
        private const int defaultShownMeasurementCount = 5;
        [ObservableProperty]
        private List<string> shownMeasurementCountList =
            Enumerable.Range(minShownMeasurementCount, maxShownMeasurementCount - minShownMeasurementCount + 1)
            .Select(i => i.ToString())
            .ToList();
        [ObservableProperty]
        private string selectedShownMeasurementCount = defaultShownMeasurementCount.ToString();
        private int _shownMeasurementCount = defaultShownMeasurementCount;

        [ObservableProperty]
        private int currentlyBufforedMeasurementCount = 0;

        [ObservableProperty]
        private List<string> valueToPlotList = new List<string>
        {
            "Prąd",
            "Opór",
        };
        [ObservableProperty]
        private string selectedValueToPlot = "Prąd";
        private string _valueToPlot = "Prąd";
        private Dictionary<string, string> _valueToPlotUnitMap = new Dictionary<string, string>
        {
            ["Prąd"] = "A",
            ["Opór"] = "Ohm",
        };

        [ObservableProperty]
        private List<string> axisTypeList = new List<string>
        {
            "Liniowa",
            "Logarytmiczna"
        };
        private Dictionary<string, AxisType> _axisTypeMap = new Dictionary<string, AxisType>
        {
            ["Liniowa"] = AxisType.Linear,
            ["Logarytmiczna"] = AxisType.Logarytmic
        };
        [ObservableProperty]
        private string selectedXAxisType = "Liniowa";
        [ObservableProperty]
        private string selectedYAxisType = "Liniowa";

        public RelayCommand ChangePlotParametersCommand { get; }
        public RelayCommand GenerateDataCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        /// <summary>Funkcja zmienia parametry wykresu</summary>
        private void ChangePlotParameters()
        {
            _shownMeasurementCount = int.Parse(SelectedShownMeasurementCount);
            _valueToPlot = SelectedValueToPlot;
            GenerateNewPlotModel();
            PlotAvalibleData();
        }

        private void GenerateData()
        {
            KithleySweeperInitData sweeperInitData = new KithleySweeperInitData()
            {
                ConnectionType = Kithley.ConnectionType.TwoWireTerminal,
                VoltageSegmentation = Kithley.VoltageSegmentation.Logarytmic,
                VoltageStart = "1",
                VoltageStop = "10",
                // VoltageStep = "1",
                CurrentProtection = "0.01",
                VoltagePoints = 10,
                SourceDelay = 0.05
            };

            for (int i=0; i<10; i++)
            {
                BootStrapper.GpibController.PushCommand(GpibCommands.Sweep, new List<object>() { sweeperInitData });
            }

            //MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, "Wywołano generowanie danych\nProszę czekać ...");
        }

        // Funkcje eventów
        // --------------------------------------------------

        /// <summary>
        /// Funkcja plotuje nowy pomiar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PlotNewMeasuermentEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                CurrentlyBufforedMeasurementCount = BootStrapper.GpibController.GetMeasurementsCount();
                if (CurrentlyBufforedMeasurementCount == 0)
                {
                    // Nastąpiło wyczyszczenie buforów
                    // Wykonywanie akcji czyszczenia wykresu
                    for (int i = 0; i < _shownMeasurementCount; i++)
                        PlotModel.PushSeriesData(i, new List<Tuple<double, double>>());
                }
                else
                    PlotAvalibleData();
            });
        }

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Funkcja służąca do generowania nowego modelu wykresu przy zmianie jego parametrów
        /// </summary>
        private void GenerateNewPlotModel()
        {
            List<SeriesInitData> newSeries = new List<SeriesInitData>();
            if (_shownMeasurementCount >= 1)
            {
                newSeries.Add(
                    new SeriesInitData(
                        0,
                        "Ostatni pomiar",
                        OxyPlot.OxyColors.Red,
                        SeriesType.Line)
                    );
            }
            for (int i = 1; i < _shownMeasurementCount; i++)
            {
                newSeries.Add(
                    new SeriesInitData(
                        i,
                        $"Pomiar -{i}",
                        OxyPlot.OxyColor.FromArgb( // Wykorzystano cieniowanie koloru pomarańczowego rgb(255, 131, 0)
                            (byte)(65 + (190 / (_shownMeasurementCount - 1)) * (_shownMeasurementCount - i)),
                            (byte)(255 * (1 - (0.75 / _shownMeasurementCount * i))),
                            (byte)(131 * (1 - (0.75 / _shownMeasurementCount * i))),
                            0
                            ),
                        SeriesType.Line)
                    );
            }
            PlotModel = new MultiSeriesPlotModel(
                title: "Ostatnie pomiary",
                xLabel: "Napięcie [V]",
                yLabel: $"{_valueToPlot} [{_valueToPlotUnitMap[_valueToPlot]}]",
                series: newSeries,
                xAxis: _axisTypeMap[SelectedXAxisType],
                yAxis: _axisTypeMap[SelectedYAxisType]
                );
        }

        /// <summary>
        /// Funkcja wyświetla dane jeżeli te są dostępne
        /// </summary>
        private void PlotAvalibleData()
        {
            // Pobieranie danych
            List<KithleyMeasurement> data = BootStrapper.GpibController.TryGetLastMeasurements(_shownMeasurementCount);
            // Plotownaie pomiarów
            for (int i = 0; i < data.Count; i++)
            {
                double[] yValue;
                switch (_valueToPlot)
                {
                    case "Prąd":
                        yValue = data[i].Current;
                        break;
                    default: // Opór
                        yValue = data[i].Resistance;
                        break;
                }
                List<Tuple<double, double>> seriesData = new List<Tuple<double, double>>(data[i].Voltage.Length);
                for (int j = 0; j < data[i].Voltage.Length; j++)
                    seriesData.Add(new Tuple<double, double>(data[i].Voltage[j], yValue[j]));
                plotModel.PushSeriesData(i, seriesData);
            }
        }

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            BootStrapper.GpibController.NewMeasuermentEvent -= PlotNewMeasuermentEvent;
        }
    }
}
