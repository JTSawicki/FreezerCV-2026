using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using LabControlsWPF.Plot3D;
using LabServices.GpibHardware;
using System.Windows;

namespace FreezerGUI.ViewModels
{
    public partial class Measurement3DPlotVM : ObservableObject, IDisposable
    {
        public Measurement3DPlotVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów
            BootStrapper.GpibController.NewMeasuermentEvent += PlotNewMeasuermentEvent;

            // Inicjalizowanie komend
            GeneratePlotCommand = new RelayCommand(GeneratePlot);
        }

        public void InitializeDefaultValues()
        {
            ;
        }

        // Pola
        // --------------------------------------------------

        [ObservableProperty]
        private List<string> axisTypeList = new List<string> { "Liniowa", "Logarytmiczna" };
        private Dictionary<string, AxisType> _axisTypeMap = new Dictionary<string, AxisType>
        {
            ["Liniowa"] = AxisType.Linear,
            ["Logarytmiczna"] = AxisType.Logarytmic
        };
        [ObservableProperty]
        private string selectedXAxisType = "Liniowa";
        [ObservableProperty]
        private string selectedYAxisType = "Liniowa";
        [ObservableProperty]
        private string selectedZAxisType = "Liniowa";

        [ObservableProperty]
        private List<string> plotTypeList = new List<string> { "Czas-Napięcie", "Temperatura-Napięcie" };
        [ObservableProperty]
        private string selectedPlotType = "Czas-Napięcie";
        [ObservableProperty]
        private List<string> plotedParameterList = new List<string> { "Prąd", "Opór" };
        [ObservableProperty]
        private string selectedPlotedParameter = "Prąd";

        [ObservableProperty]
        private string xAxisTitle = "Oś X";
        [ObservableProperty]
        private string yAxisTitle = "Oś Y";
        [ObservableProperty]
        private string zAxisTitle = "Oś Z";

        [ObservableProperty]
        private AxisType xAxisType = AxisType.Linear;
        [ObservableProperty]
        private AxisType yAxisType = AxisType.Linear;
        [ObservableProperty]
        private AxisType zAxisType = AxisType.Linear;

        [ObservableProperty]
        private bool plotXInfoLines = true;
        [ObservableProperty]
        private bool plotYInfoLines = true;
        [ObservableProperty]
        private bool plotVerdicalInfoLines = false;

        [ObservableProperty]
        private bool livePlot = false;
        [ObservableProperty]
        private bool plotByLight = false;

        /// <summary>Oświetlenie wykresu</summary>
        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                // group.Children.Add(new AmbientLight(Colors.White));
                group.Children.Add(new AmbientLight(Colors.Gray));
                group.Children.Add(new PointLight(Colors.Red, new Point3D(0, -1000, 0)));
                group.Children.Add(new PointLight(Colors.Blue, new Point3D(0, 0, 1000)));
                group.Children.Add(new PointLight(Colors.Green, new Point3D(1000, 1000, 0)));
                return group;
            }
        }

        /// <summary>Lista plotowanych punktów</summary>
        [ObservableProperty]
        private Point3D[,] data;
        [ObservableProperty]
        private object plotInvalidateFlag;

        public RelayCommand GeneratePlotCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        /// <summary>
        /// Funkcja generuje wykres
        /// </summary>
        private void GeneratePlot()
        {
            // Podpisy osi X i Y
            if (SelectedPlotType.Equals("Czas-Napięcie"))
            {
                XAxisTitle = "Czas [s]";
                YAxisTitle = "Napięcie [V]";
            }
            else // "Temperatura-Częstotliwość"
            {
                XAxisTitle = "Temperatura [°C]";
                YAxisTitle = "Napięcie [V]";
            }

            // Podpis osi Z
            if (SelectedPlotedParameter.Equals("Prąd"))
                ZAxisTitle = "Prąd [A]";
            else if (SelectedPlotedParameter.Equals("Opór"))
                ZAxisTitle = "Opór [Ohm]";

            // Typy osi
            XAxisType = _axisTypeMap[SelectedXAxisType];
            YAxisType = _axisTypeMap[SelectedYAxisType];
            ZAxisType = _axisTypeMap[SelectedZAxisType];

            // Plotowane dane
            Data = GetPointsToPlot();

            PlotInvalidateFlag = new object();
            // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, "Generowanie wykresu");
        }

        // Funkcje eventów
        // --------------------------------------------------

        /// <summary>
        /// Funkcja eventu wywoływana przy nowym pomiarze
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PlotNewMeasuermentEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() => {
                if (LivePlot && BootStrapper.GpibController.GetMeasurementsCount() >= 3)
                {
                    // Wykonywanie nowego wykresu
                    GeneratePlot();
                }
            });
        }

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Funkcja uzyskuje punkty pomiarów do splotowania
        /// </summary>
        /// <returns></returns>
        public Point3D[,] GetPointsToPlot()
        {
            List<KithleyMeasurement> measurements = BootStrapper.GpibController.GetAllMeasurements();
            if (measurements.Count == 0)
            {
                // Brak danych do plotowania
                return new Point3D[0, 0];
            }

            // Sortowanie punktów temperatury
            if (SelectedPlotType.Equals("Temperatura-Częstotliwość"))
            {
                measurements.Sort(CompareKithleyMeasurementByTemperature);
                List<KithleyMeasurement> measurementsToRemove = new List<KithleyMeasurement>();
                if (measurements.Count > 1)
                    for (int i = 0; i < measurements.Count - 1; i++)
                        if (measurements[i].Temperature == measurements[i + 1].Temperature)
                            measurementsToRemove.Add(measurements[i]);
                foreach (KithleyMeasurement elem in measurementsToRemove)
                    measurements.Remove(elem);
            }

            // Point3D[,] result = new Point3D[measurements.Count, measurements[0].Freq.Length];
            Point3D[,] result = new Point3D[measurements[0].Voltage.Length, measurements.Count];
            for (int i = 0; i < measurements.Count; i++)
                for (int j = 0; j < measurements[0].Voltage.Length; j++)
                {
                    double x;
                    double y = measurements[0].Voltage[j];
                    double z;
                    // Wybieranie danej osi x
                    if (SelectedPlotType.Equals("Czas-Napięcie"))
                    {
                        // x = (measurements[0].TimeStamp - measurements[i].TimeStamp).TotalSeconds;
                        x = (measurements[measurements.Count - 1].TimeStamp - measurements[measurements.Count - 1 - i].TimeStamp).TotalSeconds;
                    }
                    else // "Temperatura-Częstotliwość"
                    {
                        x = measurements[measurements.Count - 1 - i].Temperature;
                    }
                    // Wybieranie danej osi z
                    if (SelectedPlotedParameter.Equals("Prąd"))
                    {
                        z = measurements[measurements.Count - 1 - i].Current[j];
                    }
                    else  // "Opór"
                    {
                        z = measurements[measurements.Count - 1 - i].Resistance[j];
                    }
                    result[j, i] = new Point3D(x, y, z);
                }
            return result;
        }

        /// <summary>
        /// Funkcja porównuje elementy KithleyMeasurement po wartości temperatury
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static int CompareKithleyMeasurementByTemperature(KithleyMeasurement first, KithleyMeasurement second)
        {
            if (first.Temperature < second.Temperature)
                return -1;
            if (first.Temperature > second.Temperature)
                return 1;
            return 0;
        }

        /// <summary>
        /// Funkcja tworzy testową siatkę punktów
        /// </summary>
        /// <returns></returns>
        private static Point3D[,] CreateTestArray()
        {
            int Rows = 50;
            int Columns = 100;
            double MinX = 0;
            double MaxX = 5;
            double MinY = 0;
            double MaxY = 5;
            Point3D[,] data = new Point3D[Rows, Columns];
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    double x = MinX + (double)j / (Columns - 1) * (MaxX - MinX);
                    double y = MinY + (double)i / (Rows - 1) * (MaxY - MinY);
                    data[i, j] = new Point3D(
                        x * 5,
                        y,
                        Math.Sin(x * y) * 10
                        // x * y
                        );
                }

            return data;
        }

        /// <summary>
        /// Funkcja zapisuje do pliku siatkę punktów
        /// </summary>
        /// <param name="mesh"></param>
        private static void SaveMesh(Point3D[,] mesh)
        {
            StringBuilder builder = new StringBuilder();
            int rows = mesh.GetUpperBound(0) + 1;
            int columns = mesh.GetUpperBound(1) + 1;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    builder.Append($"({mesh[i, j].X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{mesh[i, j].Y.ToString(System.Globalization.CultureInfo.InvariantCulture)},{mesh[i, j].Z.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
                }
                builder.Append("\n");
            }
            string saveFile = FreezerM.SaveManager.GetExampleSaveFileName("Mesh save", "txt", FreezerM.SaveManager.AppFolder_UserData);
            FreezerM.SaveManager.WritetoFile(
                FreezerM.SaveManager.AppFolder_UserData + FreezerM.SaveManager.DirectorySeparator + saveFile,
                builder.ToString()
                );
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
