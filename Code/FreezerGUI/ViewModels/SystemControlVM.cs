using CommunityToolkit.Mvvm.ComponentModel;
using LabControlsWPF.AutoPid;
using LabControlsWPF.Plot2D;
using LabControlsWPF.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using FreezerM;
using LabControlsWPF;
using System.Text.Json;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using FreezerM.CodeProcesor;
using LabServices.GpibHardware;
using FreezerM.Exceptions;
using LabControlsWPF.Exceptions;
using System.Windows.Media.TextFormatting;
using Serilog;

namespace FreezerGUI.ViewModels
{
    public partial class SystemControlVM : ObservableObject, IDisposable
    {
        public SystemControlVM()
        {
            // Inicjowanie wykresu
            plotModel = new MultiSeriesPlotModel(
                title: "Przewidywany przebieg",
                xLabel: "Czas",
                yLabel: "Temperatura",
                series: new List<SeriesInitData>
                {
                    new SeriesInitData(
                        (int)PlotSeriesId.EstimatedTemperature,
                        "Przebieg temperatury",
                        OxyPlot.OxyColors.Orange,
                        SeriesType.Line),
                    new SeriesInitData(
                        (int)PlotSeriesId.Measurements,
                        "Pomiary",
                        OxyPlot.OxyColors.Red,
                        SeriesType.Scatter)
                });

            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów
            CodeInterpreter.NewCurrentlyInterpretedLineEvent += NewCurrentlyInterpretedLineEvent;

            // Inicjalizowanie komend
            LoadCodeCommand = new RelayCommand(LoadCode);
            SaveCodeCommand = new RelayCommand(SaveCode);
            LoadPidCommand = new RelayCommand(LoadPid);
            SavePidCommand = new RelayCommand(SavePid);
            LoadMeasurementDataFromFileCommand = new RelayCommand(LoadMeasurementDataFromFile);
            CheckCodeCommand = new RelayCommand(CheckCode);
            RunCodeCommand = new RelayCommand(RunCode);
            HideErrorInfoBlockCommand = new RelayCommand(HideErrorInfoBlock);
            SelectSaveFileCommand = new RelayCommand(SelectSaveFile);
        }

        public void InitializeDefaultValues()
        {
            SelectedVoltageStart = "1";
            SelectedVoltageStartUnit = UnitList[0];

            SelectedVoltageStop = "10";
            SelectedVoltageStopUnit = UnitList[0];

            SelectedCurrentProtection = "10";
            SelectedCurrentProtectionUnit = UnitList[1];

            SelectedSourceDelay = "10";
            SelectedSourceDelayUnit = UnitList[1];

            SelectedVoltageSegmentation = VoltageSegmentationList[0];
            SelectedConnectionType = ConnectionTypeList[0];

            VoltageSegmentInfo = _voltageSegmentInfoMap[_voltageSegmentationMap[SelectedVoltageSegmentation]];
            SelectedVoltageSegment = "10";
            SelectedVoltageSegmentUnit = UnitList[0];
            SelectedVoltageSegmentUnitVisibility = false;

            SelectedTemperatureUnit = TemperatureUnitList[0];
            SelectedControlType = ControlTypeList[3];
        }

        // Pola ogólne
        // --------------------------------------------------

        /// <summary>Zmienna blokująca interfejs podczas wykonywania kodu</summary>
        [ObservableProperty]
        private bool isEditInterfaceEnabled = true;

        [ObservableProperty]
        private MultiSeriesPlotModel plotModel;

        [ObservableProperty]
        private string programCode = "# Grupy komend: func, keithley, lake" + Environment.NewLine;
        /// <summary>
        /// Obecnie wykonywana linia kodu.
        /// Linia -1 oznacza brak wykonywania lini kodu
        /// </summary>
        [ObservableProperty]
        private int currentlyExecutedLine = -1;
        [ObservableProperty]
        private HintPool editorHintPool = GenerateHintPool();

        [ObservableProperty]
        private bool isAutoPid = false;
        public Action<AutoPidPool>? SetAutoPidPool;
        public Func<AutoPidPool>? GetAutoPidPool;

        [ObservableProperty]
        private string errorText = "";
        [ObservableProperty]
        private Brush errorTextBrush = new SolidColorBrush(Colors.Red);
        [ObservableProperty]
        private Visibility errorTextVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string selectedOutputFile = "";
        [ObservableProperty]
        private string selectedOutputFileInfo = "Nie wybrano";
        [ObservableProperty]
        private Brush selectedOutputFileInfoBrush = new SolidColorBrush(Colors.Red);

        public RelayCommand LoadCodeCommand { get; }
        public RelayCommand SaveCodeCommand { get; }
        public RelayCommand LoadPidCommand { get; }
        public RelayCommand SavePidCommand { get; }
        public RelayCommand CheckCodeCommand { get; }
        public RelayCommand RunCodeCommand { get; }
        public RelayCommand LoadMeasurementDataFromFileCommand { get; }
        public RelayCommand SelectSaveFileCommand { get; }
        public RelayCommand HideErrorInfoBlockCommand { get; }

        // Pola parametrów nastaw sprzętu
        // --------------------------------------------------

        [ObservableProperty]
        private string selectedVoltageStart;
        [ObservableProperty]
        private string selectedVoltageStartUnit;

        [ObservableProperty]
        private string selectedVoltageStop;
        [ObservableProperty]
        private string selectedVoltageStopUnit;

        [ObservableProperty]
        private string selectedCurrentProtection;
        [ObservableProperty]
        private string selectedCurrentProtectionUnit;

        [ObservableProperty]
        private string selectedSourceDelay;
        [ObservableProperty]
        private string selectedSourceDelayUnit;

        [ObservableProperty]
        private List<string> unitList = new List<string> { "brak", "m", "µ", "n" };
        private Dictionary<string, double> _unitListMap = new Dictionary<string, double>()
        {
            ["brak"] = 1,
            ["m"] = 0.001,
            ["µ"] = 0.000001,
            ["n"] = 0.000000001,
        };

        [ObservableProperty]
        private List<string> voltageSegmentationList = new List<string> { "Log10", "Lin" };
        private Dictionary<string, Kithley.VoltageSegmentation> _voltageSegmentationMap = new Dictionary<string, Kithley.VoltageSegmentation>
        {
            ["Log10"] = Kithley.VoltageSegmentation.Logarytmic,
            ["Lin"] = Kithley.VoltageSegmentation.Linear
        };
        public string SelectedVoltageSegmentation
        {
            get => selectedVoltageSegmentation;
            set
            {
                SetProperty(ref selectedVoltageSegmentation, value);
                SelectedVoltageSegment = "0";
                VoltageSegmentInfo = _voltageSegmentInfoMap[_voltageSegmentationMap[SelectedVoltageSegmentation]];
                if (_voltageSegmentationMap[SelectedVoltageSegmentation] == Kithley.VoltageSegmentation.Logarytmic)
                    SelectedVoltageSegmentUnitVisibility = false;
                else
                    SelectedVoltageSegmentUnitVisibility = true;
            }
        }
        private string selectedVoltageSegmentation;

        [ObservableProperty]
        private List<string> connectionTypeList = new List<string> { "4 przewody", "2 przewody" };
        private Dictionary<string, Kithley.ConnectionType> _connectionTypeMap = new Dictionary<string, Kithley.ConnectionType>
        {
            ["4 przewody"] = Kithley.ConnectionType.FourWireTerminal,
            ["2 przewody"] = Kithley.ConnectionType.TwoWireTerminal
        };
        [ObservableProperty]
        private string selectedConnectionType;

        [ObservableProperty]
        private string voltageSegmentInfo;
        private Dictionary<Kithley.VoltageSegmentation, string> _voltageSegmentInfoMap = new Dictionary<Kithley.VoltageSegmentation, string>
        {
            [Kithley.VoltageSegmentation.Logarytmic] = "Ilość punktów\nsegmentacji",
            [Kithley.VoltageSegmentation.Linear] = "Krok\nnapięcia [V]"
        };
        [ObservableProperty]
        private string selectedVoltageSegment;
        [ObservableProperty]
        private string selectedVoltageSegmentUnit;
        [ObservableProperty]
        private bool selectedVoltageSegmentUnitVisibility;

        [ObservableProperty]
        private List<string> temperatureUnitList = new List<string> { "Celsjusze", "Kelwiny" };
        private Dictionary<string, LakeShore.TemperatureUnit> _temperatureUnitMap = new Dictionary<string, LakeShore.TemperatureUnit>
        {
            ["Celsjusze"] = LakeShore.TemperatureUnit.Celcius,
            ["Kelwiny"] = LakeShore.TemperatureUnit.Kelvins,
        };
        [ObservableProperty]
        private string selectedTemperatureUnit;

        [ObservableProperty]
        private List<string> controlTypeList = new List<string> { "Ręczny PID", "Auto P", "Auto PI", "Auto PID" };
        private Dictionary<string, LakeShore.ControlType> _controlTypeMap = new Dictionary<string, LakeShore.ControlType>
        {
            ["Ręczny PID"] = LakeShore.ControlType.Manual,
            ["Auto P"] = LakeShore.ControlType.AutoP,
            ["Auto PI"] = LakeShore.ControlType.AutoPI,
            ["Auto PID"] = LakeShore.ControlType.AutoPID,
        };
        [ObservableProperty]
        private string selectedControlType;

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        /// <summary>
        /// Funkcja wczytuje z pliku kod i ustawienia jego wywołania
        /// </summary>
        private void LoadCode()
        {
            // Wybór pliku zapisu
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Plik zapisu kodu programu",
                Filter = "Piecyk save (*.FCode)|*.FCode|All files (*.*)|*.*",
                InitialDirectory = SaveManager.AppFolder_UserData,
                ValidateNames = true
            };

            // Sprawdzenie czy wybrano plik
            if (openFileDialog.ShowDialog() == false)
                return;

            // Deserializacja
            string jsonSaveString = File.ReadAllText(openFileDialog.FileName);
            CodeSaveObject? saveObject = JsonSerializer.Deserialize<CodeSaveObject>(jsonSaveString);

            // Sprawdzenie poprawności deserializacji
            if (saveObject == null)
            {
                MaterialMessageBox.NewFastMessage(MaterialMessageFastType.BadUserInputWarning, "Niepoprawny lub uszkodzony plik zapisu.");
                return;
            }

            // Ustawianie prarametrów
            IsAutoPid = saveObject.IsAutoPidActive;
            ProgramCode = saveObject.ProgramCode;
            SelectedVoltageSegmentation = saveObject.VoltageSegmentation;
            SelectedConnectionType = saveObject.ConnectionType;
            SelectedVoltageStart = saveObject.VoltageStart;
            SelectedVoltageStartUnit = saveObject.VoltageStartUnit;
            SelectedVoltageStop = saveObject.VoltageStop;
            SelectedVoltageStopUnit = saveObject.VoltageStopUnit;
            SelectedVoltageSegment = saveObject.VoltageSegment;
            SelectedVoltageSegmentUnit = saveObject.VoltageSegmentUnit;
            SelectedCurrentProtection = saveObject.CurrentProtection;
            SelectedCurrentProtectionUnit = saveObject.CurrentProtectionUnit;
            SelectedSourceDelay = saveObject.SourceDelay;
            SelectedSourceDelayUnit = saveObject.SourceDelayUnit;
        }

        /// <summary>
        /// Funkcja zapisuje do pliku kod i ustawienia jego wywołania
        /// </summary>
        private void SaveCode()
        {
            string jsonSaveString = GetCodeSave(true);
            // Okno wyboru pliku
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Plik zapisu kodu programu",
                FileName = SaveManager.GetExampleSaveFileName("FreezerProgram", "FCode", SaveManager.AppFolder_UserData),
                AddExtension = true,
                Filter = "Piecyk save (*.FCode)|*.FCode|All files (*.*)|*.*",
                InitialDirectory = SaveManager.AppFolder_UserData,
                ValidateNames = true
            };
            // Zapis danych jeżeli wybrano plik
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, jsonSaveString);
            }
        }

        /// <summary>
        /// Funkcja wczytuje z pliku ustawienia auto pid
        /// </summary>
        private void LoadPid()
        {
            if (SetAutoPidPool == null)
                return;
            // Wybór pliku zapisu
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Plik zapisu parametrów auto pid",
                Filter = "Freezer pid save (*.FPid)|*.FPid|All files (*.*)|*.*",
                InitialDirectory = SaveManager.AppFolder_UserData,
                ValidateNames = true
            };
            // Deserializacja jeżeli wybrano plik
            if (openFileDialog.ShowDialog() == true)
            {
                string jsonSaveString = File.ReadAllText(openFileDialog.FileName);
                AutoPidPool? saveObject =
                    JsonSerializer.Deserialize<AutoPidPool>(jsonSaveString);
                // Sprawdzenei poprawności deserializacji
                if (saveObject == null)
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.BadUserInputWarning, "Niepoprawny lub uszkodzony plik zapisu");
                    return;
                }
                SetAutoPidPool(saveObject);
            }
        }

        /// <summary>
        /// Funkcja zapisuje do pliku ustawienia auto pid
        /// </summary>
        private void SavePid()
        {
            if (GetAutoPidPool == null)
                return;
            string jsonSaveString = GetAutoPidSave(true);
            // Okno wyboru pliku
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Plik zapisu parametrów auto pid",
                FileName = SaveManager.GetExampleSaveFileName("FreezerAutoPidParameters", "FPid", BootStrapper.Settings.DefaultSaveFolder),
                AddExtension = true,
                Filter = "Freezer pid save (*.FPid)|*.FPid|All files (*.*)|*.*",
                InitialDirectory = SaveManager.AppFolder_UserData,
                ValidateNames = true
            };
            // Zapis danych jeżeli wybrano plik
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, jsonSaveString);
            }
        }

        /// <summary>
        /// Funkcja sprawdza poprawność kodu parametrów i rysuje wykres estymacji przebiegu
        /// </summary>
        private void CheckCode()
        {
            InternalCheckProgram();
        }

        /// <summary>
        /// Funkcja wywołuje kod
        /// </summary>
        private void RunCode()
        {
            // Kontrola poprawności danych wejściowych
            bool isProgramGodToRun = InternalCheckProgram();
            if (!isProgramGodToRun)
            {
                MaterialMessageBox.NewFastMessage(MaterialMessageFastType.BadUserInputWarning, "Ze względu na niepoprawnie wprowadzone dane nie można uruchomić kodu 😢");
                return;
            }

            // Czyszczenie buforów pomiarów z poprzedniego programu
            if (MaterialMessageBox.NewFastMessage(MaterialMessageFastType.ConfirmActionInfo, "Uruchomienie programu.\nWyczyści to bufory pomiarów.", true) == true)
            {
                BootStrapper.GpibController.ClearMeasurementPool();
            }
            else
                return;

            // Logowanie wywołania kodu
            Log.Information("SystemControlViewModel.RunCode-Executing code");
            Log.Information($"SystemControlViewModel.RunCode-Program: {GetCodeSave(false)}");
            if (IsAutoPid)
            {
                Log.Information($"SystemControlViewModel.RunCode-AutoPid ON: {GetAutoPidSave(false)}");
            }

            // Konstruowanie i wysyłanie obiektu ustawień sweepera
            var converted = GetConvertedValues();
            KithleySweeperInitData sweeperInitData = new KithleySweeperInitData()
            {
                ConnectionType = converted.ConnectionType,
                VoltageSegmentation = converted.VoltageSegmentation,
                VoltageStart = converted.VoltageStart.ToString().Replace(',', '.'),
                VoltageStop = converted.VoltageStop.ToString().Replace(',', '.'),

                VoltageStep = converted.VoltageSegmentation == Kithley.VoltageSegmentation.Logarytmic ? "1" : converted.VoltageSegment.ToString().Replace(',', '.'),
                VoltagePoints = converted.VoltageSegmentation == Kithley.VoltageSegmentation.Logarytmic ? int.Parse(SelectedVoltageSegment) : 10,

                CurrentProtection = converted.CurrentProtection.ToString().Replace(',', '.'),
                SourceDelay = converted.SourceDelay,
            };
            BootStrapper.GpibController.PushCommand(
                GpibCommands.SetSweeperParameters,
                new List<object>
                {
                    sweeperInitData
                });

            // Wysyłanie ustawień auto pid
            if (IsAutoPid)
            {
                AutoPidPool tmpPool = GetAutoPidPool!.Invoke();
                Tuple<ushort, ushort, ushort> tmpDefaultPid = tmpPool.GetDefaultPid();
                SortedList<double, Tuple<ushort, ushort, ushort>> tmpPidPool = tmpPool.GetPidPool();

                LakeShorePidValue defaultPid = new LakeShorePidValue(tmpDefaultPid.Item1, tmpDefaultPid.Item2, tmpDefaultPid.Item3);
                List<KeyValuePair<double, LakeShorePidValue>> pidPool = new List<KeyValuePair<double, LakeShorePidValue>>();
                foreach (double key in tmpPidPool.Keys)
                {
                    Tuple<ushort, ushort, ushort> tmpPid = tmpPidPool[key];
                    LakeShorePidValue pid = new LakeShorePidValue(tmpPid.Item1, tmpPid.Item2, tmpPid.Item3);
                    pidPool.Add(new KeyValuePair<double, LakeShorePidValue>(key, pid));
                }
                LakeShoreAutoPidPool pool = new LakeShoreAutoPidPool(pidPool, defaultPid);
                BootStrapper.GpibController.PushCommand(
                    GpibCommands.SetAutoPid,
                    new List<object> { pool }
                    );
            }
            else
            {
                // Wyłączanie autopid jeżeli jest włączony
                BootStrapper.GpibController.PushCommand(GpibCommands.SetAutoPid);
            }

            // Wysyłanie nastaw LakeShore
            BootStrapper.GpibController.PushCommand(
                GpibCommands.SetLakeShoreControlMode,
                new List<object> { _controlTypeMap[SelectedControlType] }
                );
            BootStrapper.GpibController.PushCommand(
                GpibCommands.SetLakeShoreTemperatureUnit,
                new List<object> { _temperatureUnitMap[SelectedTemperatureUnit] }
                );

            // Uruchamianie wątku interpretera programu
            List<CodeCommandContainer> code = CodePreprocessor.ProcessCode(ProgramCode, BootStrapper.CommandMaster);
            CodeLoopRunData codeLoopRunData = new CodeLoopRunData(code, BootStrapper.CommandMaster);
            CodeInterpreter.Start(codeLoopRunData);
        }

        /// <summary>
        /// Funkcja wczytuje zapisane dane pomiarowe z pliku do buforów.
        /// Pododuje to nadpisanie zawartości buforów.
        /// </summary>
        private void LoadMeasurementDataFromFile()
        {
            bool? loadConfirmation = MaterialMessageBox.NewFastMessage(
                MaterialMessageFastType.ConfirmActionInfo,
                "Wczytanie pomiarów z pliku.\nSpowoduje to nadpisanie obecnych buforów danych pomiarowych.",
                true);
            if (loadConfirmation!.Value == false)
                return;

            // Wybieranie pliku zapisu i wczytywanie danych
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Plik zapisu pomiarów",
                Filter = "Freezer save (*.tsv)|*.tsv|Freezer save (*.csv)|*.csv",
                InitialDirectory = BootStrapper.Settings.DefaultSaveFolder,
                ValidateNames = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string saveFileName = openFileDialog.FileName;
                string saveFileContent = SaveManager.ReadFile(saveFileName);
                bool conversionSuccesFlag;
                if (saveFileName.Substring(saveFileName.Length - 3).Equals("tsv"))
                    conversionSuccesFlag = BootStrapper.GpibController.LoadFromTSV(saveFileContent);
                else if (saveFileName.Substring(saveFileName.Length - 3).Equals("csv"))
                    conversionSuccesFlag = BootStrapper.GpibController.LoadFromCSV(saveFileContent);
                else
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.BadUserInputWarning, "Wybrano niepoprawny plik zapisu.\nAkceptowane pliki: \"csv\", \"tsv\".");
                    return;
                }

                // Sprawdzenie czy dane zostały wczytane
                if (!conversionSuccesFlag)
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, "Nie udało się wczytać pliku danych.");
                    return;
                }
                else
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, "Wczytano dane.");
                    return;
                }
            }
        }

        /// <summary>
        /// Funkcja ukrywa blok informacji o błędach kodu
        /// </summary>
        private void HideErrorInfoBlock()
        {
            ErrorTextVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Funkcja uruchamia okno wyboru pliku zapisu pomiarów
        /// </summary>
        private void SelectSaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Plik zapisu pomiarów",
                FileName = SaveManager.GetExampleSaveFileName("Measurements", "tsv", BootStrapper.Settings.DefaultSaveFolder),
                AddExtension = true,
                Filter = "Freezer save (*.tsv)|*.tsv|Freezer save (*.csv)|*.csv|All files (*.*)|*.*",
                InitialDirectory = BootStrapper.Settings.DefaultSaveFolder,
                ValidateNames = true
            };
            // Zapis wybranego pliku
            if (saveFileDialog.ShowDialog() == true)
            {
                SelectedOutputFileInfoBrush = new SolidColorBrush(Colors.Black);
                SelectedOutputFile = saveFileDialog.FileName;
                // Skracanie wyświetlanej nazwy
                if (saveFileDialog.FileName.Length > 60)
                {
                    SelectedOutputFileInfo =
                        saveFileDialog.FileName.Substring(0, 20) +
                        " ... " +
                        saveFileDialog.FileName.Substring(saveFileDialog.FileName.Length - 30);
                }
                else
                    SelectedOutputFileInfo = saveFileDialog.FileName;

                // Zapisywanie nowej domyślnej lokalizacji dla plików
                if (BootStrapper.Settings.SaveLastAsDefaultSaveFolder == true)
                {
                    BootStrapper.Settings.DefaultSaveFolder = Path.GetDirectoryName(SelectedOutputFile)!;
                    BootStrapper.Settings.Save();
                }
            }
        }

        // Funkcje eventów
        // --------------------------------------------------

        /// <summary>
        /// Funkcja odświeżająca obecnie wykonywaną linię kodu i ustawiająca blokadę dla interfejsu na czas interpretacji kodu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCurrentlyInterpretedLineEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                int newCurrentlyExecutedLine = CodeInterpreter.GetCurrentlyInterpretedLine();
                // Blokowanie interfejsu
                if (CurrentlyExecutedLine == -1 &&
                    newCurrentlyExecutedLine != -1)
                    IsEditInterfaceEnabled = false;
                if (newCurrentlyExecutedLine == -1)
                {
                    // Zakończenie programu/pomiarów
                    string dataToSave;
                    if (SelectedOutputFile.Substring(SelectedOutputFile.Length - 3).Equals("csv"))
                        dataToSave = BootStrapper.GpibController.GenerateCSV();
                    else
                        dataToSave = BootStrapper.GpibController.GenerateTSV();
                    if (!string.IsNullOrEmpty(dataToSave))
                    {
                        SaveManager.WritetoFile(SelectedOutputFile, dataToSave);
                        MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, "Program zakończył działanie.\nPomiary zapisane.");
                    }
                    else
                        MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, "Program zakończył działanie.\nNie wykonano pomiarów brak pliku zapisu.");
                    IsEditInterfaceEnabled = true;
                }
                // Zmiana wykonywanej lini kodu
                CurrentlyExecutedLine = newCurrentlyExecutedLine;
            });
        }

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Funkcja zwraca string zapisu autopid
        /// </summary>
        /// <param name="writeIntended">Czy plik ma nyć czytelny dla człowieka(obecne wcięcia, spacje itp.)</param>
        /// <returns>String zapisu lub pusty jeżeli brak dostępu</returns>
        private string GetAutoPidSave(bool writeIntended)
        {
            if (GetAutoPidPool == null)
                return "";
            // Serializacja
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = writeIntended
            };
            return JsonSerializer.Serialize(GetAutoPidPool(), serializerOptions);
        }

        /// <summary>
        /// Funkcja generuje pulę podpowiedzi dla edytora tekstu.
        /// </summary>
        /// <returns></returns>
        private static HintPool GenerateHintPool()
        {
            HintPool pool = new HintPool();
            foreach (string group in BootStrapper.CommandMaster.GetGroupNames())
            {
                List<MyCompletionData> commandCompletionData = new List<MyCompletionData>();
                CommandGroup commandGroup = BootStrapper.CommandMaster.GetCommandGroup(group);
                foreach (string command in commandGroup.GetCommandNames())
                {
                    commandCompletionData.Add(MyCompletionData.GetCommandCompletionData(
                        command: command,
                        commandGroup: group,
                        parameterCount: commandGroup.GetParametersInfo(command).Count(),
                        description: commandGroup.GetShortCommandDescription(command),
                        additionalText: commandGroup.GetAdditionalTextToInsert(command)
                        ));
                }
                pool.RegisterGroup(
                    groupName: group,
                    commandHints: commandCompletionData,
                    groupDescription: BootStrapper.CommandMaster.GetCommandGroup(group).Description
                    );
            }
            return pool;
        }

        /// <summary>
        /// Funkcja zwraca string zapisu programu
        /// </summary>
        /// <param name="writeIntended">Czy plik ma nyć czytelny dla człowieka(obecne wcięcia, spacje itp.)</param>
        /// <returns>String zapisu</returns>
        private string GetCodeSave(bool writeIntended)
        {
            var saveObject = new CodeSaveObject(
                isAutoPidActive: IsAutoPid,
                programCode: ProgramCode,
                voltageSegmentation: SelectedVoltageSegmentation,
                connectionType: SelectedConnectionType,
                voltageStart: SelectedVoltageStart,
                voltageStartUnit: SelectedVoltageStartUnit,
                voltageStop: SelectedVoltageStop,
                voltageStopUnit: SelectedVoltageStopUnit,
                voltageSegment: SelectedVoltageSegment,
                voltageSegmentUnit: SelectedVoltageSegmentUnit,
                currentProtection: SelectedCurrentProtection,
                currentProtectionUnit: SelectedCurrentProtectionUnit,
                sourceDelay: SelectedSourceDelay,
                sourceDelayUnit: SelectedSourceDelayUnit
                );

            // Serializacja
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = writeIntended
            };
            return JsonSerializer.Serialize(saveObject, serializerOptions);
        }

        /// <summary>
        /// Funckja sprawdza poprawność programu i ustawień wprowadzonych przez użytkownika
        /// </summary>
        /// <returns>Czy program jest poprawny</returns>
        private bool InternalCheckProgram()
        {
            // Tworzenie wiadomości zwrotnej
            StringBuilder errorMessage = new StringBuilder();

            // Kontrola kodu programu
            try
            {
                List<CodeCommandContainer> commands = CodePreprocessor.ProcessCode(ProgramCode, BootStrapper.CommandMaster);
                RunEstimatorContainer estimation = CodePreprocessor.GenerateEstimation(commands, BootStrapper.CommandMaster);
                PlotModel.PushSeriesData((int)PlotSeriesId.EstimatedTemperature, estimation.Temperatures);
                PlotModel.PushSeriesData((int)PlotSeriesId.Measurements, estimation.Measurements);
            }
            catch (CodePreprocessorErrorException ex)
            {
                errorMessage.AppendLine(ex.Message);
                PlotModel.PushSeriesData((int)PlotSeriesId.EstimatedTemperature, new List<Tuple<double, double>>());
                PlotModel.PushSeriesData((int)PlotSeriesId.Measurements, new List<Tuple<double, double>>());
            }

            // Kontrola parametrów auto pid
            string autoPidCheckMessage = CheckAutoPidInputCorrectness();
            if (!string.IsNullOrEmpty(autoPidCheckMessage))
                errorMessage.AppendLine(autoPidCheckMessage);

            // Kontrola spójności ustawień PID
            if (_controlTypeMap[SelectedControlType] != LakeShore.ControlType.Manual && IsAutoPid)
            {
                errorMessage.AppendLine($"Wybrano AutoPID i automatyczną kontrolę '{SelectedControlType}' jednocześnie");
            }

            // Kontrola poprawności wpisania parametrów wywołania
            string parametersCheckMessage = CheckParametersInput();
            if (!string.IsNullOrEmpty(parametersCheckMessage))
                errorMessage.AppendLine(parametersCheckMessage);

            // Kontrola wartości wpisanych danych
            var converted = GetConvertedValues();
            string parametersValueCheckMessage = converted.ValidateParametersValues();
            if (!string.IsNullOrEmpty(parametersValueCheckMessage))
                errorMessage.AppendLine(parametersValueCheckMessage);

            // Wyświetlanie błędu jeżeli jest taki
            if (errorMessage.Length > 0)
            {
                ErrorText = errorMessage.ToString();
                ErrorTextVisibility = Visibility.Visible;
                ErrorTextBrush = new SolidColorBrush(Colors.Red);
                return false;
            }
            else
            {
                ErrorText = "Twój program jest poprawny";
                ErrorTextVisibility = Visibility.Visible;
                ErrorTextBrush = new SolidColorBrush(Colors.Green);
                return true;
            }
        }

        /// <summary>
        /// Funkcja sprawdza poprawność wprowadzonych przez użytkownika parametrów auto pid
        /// </summary>
        /// <returns>Zwraca pusty string jeżeli poprawne. Jeżeli nie zawiera błęy do wyświetlenia</returns>
        private string CheckAutoPidInputCorrectness()
        {
            StringBuilder outputMessage = new StringBuilder();

            if (!IsAutoPid)
            {
                // Test niepotrzebny
                goto AutoPidCheckBreak;
            }

            if (GetAutoPidPool != null)
            {
                // Testowanie poprawności danych wejściowych
                AutoPidPool? tmpPool = GetAutoPidPool.Invoke();
                if (tmpPool == null)
                {
                    outputMessage.AppendLine("Wadliwe wiązanie danych autopid.");
                    goto AutoPidCheckBreak;
                }

                Tuple<ushort, ushort, ushort> defaultPid;
                SortedList<double, Tuple<ushort, ushort, ushort>> pool;
                try
                {
                    defaultPid = tmpPool.GetDefaultPid();
                    pool = tmpPool.GetPidPool();
                }
                catch (BadUserInputException ex)
                {
                    outputMessage.AppendLine("Autopid | " + ex.Message);
                    goto AutoPidCheckBreak;
                }

                // Testowanie limitów danych
                if (defaultPid.Item1 > LakeShore.MaxPParameter ||
                    defaultPid.Item2 > LakeShore.MaxIParameter ||
                    defaultPid.Item3 > LakeShore.MaxDParameter)
                    outputMessage.AppendLine("Autopid | Niepoprawne wartości domyślnej nastawy pid");
                if (pool.Count == 0)
                    outputMessage.AppendLine("Autopid | UWAGA! Wprowadzono zerową liczbę punktów autopid");
                foreach (double temperature in pool.Keys)
                    if (pool[temperature].Item1 > LakeShore.MaxPParameter ||
                        pool[temperature].Item2 > LakeShore.MaxIParameter ||
                        pool[temperature].Item3 > LakeShore.MaxDParameter)
                        outputMessage.AppendLine($"Autopid | Niepoprawne wartości nastawy auto pid dla temperatury: {temperature}");

                // Testowanie zerowej części proporcjonalnej PID
                if (defaultPid.Item1 == 0)
                    outputMessage.AppendLine("Autopid | Domyślna nastawa auto pid ma zerową część proporcjonalną. System bezwładny dla tego zakresu");
                foreach (double temperature in pool.Keys)
                    if (pool[temperature].Item1 == 0)
                        outputMessage.AppendLine($"Autopid | Nastawa auto pid dla temperatury: {temperature} ma zerową część proporcjonalną. System bezwładny dla tego zakresu");
            }
            else
                outputMessage.AppendLine("Brak możliwości pobrania danych autopid.");
            // Zamykanie testu
            AutoPidCheckBreak:;
            return outputMessage.ToString();
        }

        /// <summary>
        /// Funckja sprawdza poprawność wprowadzonych parametrów wywołania
        /// </summary>
        /// <returns></returns>
        private string CheckParametersInput()
        {
            // Tworzenie wiadomości zwrotnej
            StringBuilder outputMessage = new StringBuilder();

            // Kontrola poprawności wpisania ustawień wywołania
            object? checker = NumericConverters.StringToNumber(SelectedVoltageStart, ConvertableNumericTypes.Int);
            if (checker == null)
                outputMessage.AppendLine("Parametry | Niepoprawny literał parametru: \"Początkowe napięcie\"");
            // ----------------------------------------------------------------------------------------------------
            checker = NumericConverters.StringToNumber(SelectedVoltageStop, ConvertableNumericTypes.Int);
            if (checker == null)
                outputMessage.AppendLine("Parametry | Niepoprawny literał parametru: \"Końcowe napięcie\"");
            // ----------------------------------------------------------------------------------------------------
            checker = NumericConverters.StringToNumber(SelectedVoltageSegment, ConvertableNumericTypes.Int);
            if (checker == null)
                outputMessage.AppendLine($"Parametry | Niepoprawny literał parametru: {VoltageSegmentInfo}");
            // ----------------------------------------------------------------------------------------------------
            checker = NumericConverters.StringToNumber(SelectedCurrentProtection, ConvertableNumericTypes.Int);
            if (checker == null)
                outputMessage.AppendLine("Parametry | Niepoprawny literał parametru: \"Limit prądu\"");
            // ----------------------------------------------------------------------------------------------------
            checker = NumericConverters.StringToNumber(SelectedSourceDelay, ConvertableNumericTypes.Int);
            if (checker == null)
                outputMessage.AppendLine("Parametry | Niepoprawny literał parametru: \"Opóźnienie źródła\"");
            // ----------------------------------------------------------------------------------------------------

            // Kontrola poprawności ścieżki zapisu
            bool possiblePath = SelectedOutputFile.IndexOfAny(Path.GetInvalidPathChars()) == -1;
            if (!possiblePath || string.IsNullOrEmpty(SelectedOutputFile))
                outputMessage.AppendLine("Parametry | Niepoprawna ścieżka pliku zapisu pomiarów");
            if (File.Exists(SelectedOutputFile))
                outputMessage.AppendLine("Parametry | Plik o podanej ścieżce zapisu już istnieje");

            return outputMessage.ToString();
        }

        private ConvertedParametersObject GetConvertedValues()
        {
            return new ConvertedParametersObject(
                voltageStart: (double)NumericConverters.StringToNumber(SelectedVoltageStart, ConvertableNumericTypes.Double)! * _unitListMap[SelectedVoltageStartUnit],
                voltageStop: (double)NumericConverters.StringToNumber(SelectedVoltageStop, ConvertableNumericTypes.Double)! * _unitListMap[SelectedVoltageStopUnit],
                voltageSegment: (double)NumericConverters.StringToNumber(SelectedVoltageSegment, ConvertableNumericTypes.Double)! * _unitListMap[SelectedVoltageSegmentUnit],
                currentProtection: (double)NumericConverters.StringToNumber(SelectedCurrentProtection, ConvertableNumericTypes.Double)! * _unitListMap[SelectedCurrentProtectionUnit],
                sourceDelay: (double)NumericConverters.StringToNumber(SelectedSourceDelay, ConvertableNumericTypes.Double)! * _unitListMap[SelectedSourceDelayUnit],
                connectionType: _connectionTypeMap[SelectedConnectionType],
                voltageSegmentation: _voltageSegmentationMap[SelectedVoltageSegmentation]
                );
        }

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            ;
        }

        // Inne
        // --------------------------------------------------

        /// <summary>
        /// Wartości ID serii wykresu estymacji
        /// </summary>
        private enum PlotSeriesId : int
        {
            EstimatedTemperature,
            Measurements
        }

        /// <summary>
        /// Obiekt kontenerowy dla zapisu ustawień kodu oraz programu użytkownika
        /// </summary>
        private class CodeSaveObject
        {
            /// <summary>Zmienna na potrzeby oznaczenia wersji podczas serializacji do pliku</summary>
            public int SaveVersion { get; set; } = 1;
            public bool IsAutoPidActive { get; set; }
            public string ProgramCode { get; set; }

            public string VoltageSegmentation { get; set; }
            public string ConnectionType { get; set; }

            public string VoltageStart { get; set; }
            public string VoltageStartUnit { get; set; }
            public string VoltageStop { get; set; }
            public string VoltageStopUnit { get; set; }
            public string VoltageSegment { get; set; }
            public string VoltageSegmentUnit { get; set; }
            public string CurrentProtection { get; set; }
            public string CurrentProtectionUnit { get; set; }
            public string SourceDelay { get; set; }
            public string SourceDelayUnit { get; set; }

            public CodeSaveObject(bool isAutoPidActive, string programCode, string voltageSegmentation, string connectionType, string voltageStart, string voltageStartUnit, string voltageStop, string voltageStopUnit, string voltageSegment, string voltageSegmentUnit, string currentProtection, string currentProtectionUnit, string sourceDelay, string sourceDelayUnit)
            {
                IsAutoPidActive = isAutoPidActive;
                ProgramCode = programCode;
                VoltageSegmentation = voltageSegmentation;
                ConnectionType = connectionType;
                VoltageStart = voltageStart;
                VoltageStartUnit = voltageStartUnit;
                VoltageStop = voltageStop;
                VoltageStopUnit = voltageStopUnit;
                VoltageSegment = voltageSegment;
                VoltageSegmentUnit = voltageSegmentUnit;
                CurrentProtection = currentProtection;
                CurrentProtectionUnit = currentProtectionUnit;
                SourceDelay = sourceDelay;
                SourceDelayUnit = sourceDelayUnit;
            }
        }

        /// <summary>
        /// Obiekt kontenerowy dla przekonwertowanych wartości parameterów
        /// </summary>
        private class ConvertedParametersObject
        {
            public double VoltageStart { get; set; }
            public double VoltageStop { get; set; }
            public double VoltageSegment { get; set; }
            public double CurrentProtection { get; set; }
            public double SourceDelay { get; set; }

            public Kithley.ConnectionType ConnectionType { get; set; }
            public Kithley.VoltageSegmentation VoltageSegmentation { get; set; }

            public ConvertedParametersObject(double voltageStart, double voltageStop, double voltageSegment, double currentProtection, double sourceDelay, Kithley.ConnectionType connectionType, Kithley.VoltageSegmentation voltageSegmentation)
            {
                VoltageStart = voltageStart;
                VoltageStop = voltageStop;
                VoltageSegment = voltageSegment;
                CurrentProtection = currentProtection;
                SourceDelay = sourceDelay;
                ConnectionType = connectionType;
                VoltageSegmentation = voltageSegmentation;
            }

            /// <summary>
            /// Funkcja sprawdza poprawność wartości parametrów wywołania
            /// </summary>
            /// <returns></returns>
            public string ValidateParametersValues()
            {
                // Tworzenie wiadomości zwrotnej
                StringBuilder errorMessage = new StringBuilder();

                double tmp;

                tmp = Math.Abs(VoltageStart);
                if (tmp < Kithley.MinSourceVoltage)
                    errorMessage.AppendLine($"Zbyt mała wartość parametru \"Początkowe napięcie\". Min: +-{Kithley.MinSourceVoltage}");
                if (tmp > Kithley.MaxSourceVoltage)
                    errorMessage.AppendLine($"Zbyt duża wartość parametru \"Początkowe napięcie\". Max: +-{Kithley.MaxSourceVoltage}");

                tmp = Math.Abs(VoltageStop);
                if (tmp < Kithley.MinSourceVoltage)
                    errorMessage.AppendLine($"Zbyt mała wartość parametru \"Końcowe napięcie\". Min: +-{Kithley.MinSourceVoltage}");
                if (tmp > Kithley.MaxSourceVoltage)
                    errorMessage.AppendLine($"Zbyt duża wartość parametru \"Końcowe napięcie\". Max: +-{Kithley.MaxSourceVoltage}");

                if (VoltageSegmentation == Kithley.VoltageSegmentation.Linear)
                {
                    tmp = Math.Abs(VoltageSegment);
                    if (VoltageSegment < 0)
                        errorMessage.AppendLine($"Ujemna wartość parametru \"Ilość punktów segmentacji\"");
                    if (tmp < Kithley.MinSourceVoltage)
                        errorMessage.AppendLine($"Zbyt mała wartość parametru \"Krok napięcia\". Min: {Kithley.MinSourceVoltage}");
                    if (tmp > Kithley.MaxSourceVoltage)
                        errorMessage.AppendLine($"Zbyt duża wartość parametru \"Krok napięcia\". Max: {Kithley.MaxSourceVoltage}");

                    int points =  (int)(Math.Abs(VoltageStart - VoltageStop) / VoltageSegment);
                    if (points + 1 > Kithley.MaxMeasurementPerSweepCount)
                        errorMessage.AppendLine($"Zbyt duża ilość punktów pomiarowych w sweep. Max: {Kithley.MaxMeasurementPerSweepCount}. Wygenerowano: {points}");
                }
                else
                {
                    if (VoltageSegment == 0)
                        errorMessage.AppendLine($"Zerowa wartość parametru \"Ilość punktów segmentacji\"");
                    if (VoltageSegment < 0)
                        errorMessage.AppendLine($"Ujemna wartość parametru \"Ilość punktów segmentacji\"");
                    if (VoltageSegment > Kithley.MaxMeasurementPerSweepCount)
                        errorMessage.AppendLine($"Zbyt duża wartość parametru \"Ilość punktów segmentacji\". Max: {Kithley.MaxMeasurementPerSweepCount}");
                }

                tmp = Math.Abs(CurrentProtection);
                if (CurrentProtection < Kithley.MinSenseCurrent)
                    errorMessage.AppendLine($"Ujemna wartość parametru \"Limit prądu\"");
                if (tmp < Kithley.MinSenseCurrent)
                    errorMessage.AppendLine($"Zbyt mała wartość parametru \"Limit prądu\". Min: {Kithley.MinSenseCurrent}");
                if (tmp > Kithley.MaxSenseCurrent)
                    errorMessage.AppendLine($"Zbyt duża wartość parametru \"Limit prądu\". Max: {Kithley.MaxSenseCurrent}");

                tmp = Math.Abs(CurrentProtection) * Math.Max(Math.Abs(VoltageStart), Math.Abs(VoltageStop));
                if (tmp > Kithley.MaxPower)
                    errorMessage.AppendLine($"Zbyt duża moc maksymalna dla wyjścia. Max: {Kithley.MaxPower}W. Dla wybranych parametrów: {tmp}W");

                if (SourceDelay < 0)
                    errorMessage.AppendLine($"Ujemny czas parametru \"Opóźnienie źródła\"");
                if (SourceDelay > Kithley.MaxSourceDelay)
                    errorMessage.AppendLine($"Zbyt duża wartość parametru \"Opóźnienie źródła\". Max: {Kithley.MaxSourceDelay}");

                return errorMessage.ToString();
            }
        }
    }
}
