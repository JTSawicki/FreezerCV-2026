using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LabControlsWPF.Plot2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FreezerM;
using LabServices.GpibHardware;
using OxyPlot;

namespace FreezerGUI.ViewModels
{
    public partial class SystemStateVM : ObservableObject, IDisposable
    {
        public SystemStateVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów
            BootStrapper.GpibController.NewChamberTemperature += UpdateCurrentTemperatureInfo;
            BootStrapper.GpibController.NewSampleTemperature += UpdateCurrentSampleTemperatureInfo;
            BootStrapper.GpibController.NewLakeShoreTemperatureUnit += UpdateTemperatureUnitInfo;
            BootStrapper.GpibController.NewChamberTemperature += PlotNewPoint;
            BootStrapper.GpibController.NewTargetTemperaturePointEvent += UpdateTargetTemperatureInfo;
            BootStrapper.GpibController.NewTargetTemperaturePointEvent += PlotNewPoint;
            BootStrapper.GpibController.NewLakeShorePIDValueEvent += UpdatePidInfo;
            BootStrapper.GpibController.NewLakeShoreControlType += UpdatePidTypeInfo;

            BootStrapper.FlowSensorController.NewCurrentFlowEvent += NewCurrentFlow;

            this.PropertyChanged += ForceParametersCommandLockDedection;

            // Inicjalizowanie komend
            ForceParametersCommand = new RelayCommand(ForceParameters, CanForceParameters);
        }

        public void InitializeDefaultValues()
        {
            // Inicjalizacja modelu wykresu
            PlotModel = new RealTimePlotModel(
                title: "Historia stanu kriostatu",
                yLabel: "Temperatura",
                new List<SeriesInitData>
                {
                    new SeriesInitData((int)PlotSeriesID.TargetTemperature, "Nastawa temperatury", OxyColors.Green, SeriesType.Line),
                    new SeriesInitData((int)PlotSeriesID.CurrentTemperature, "Obecna temperata", OxyColors.Red, SeriesType.Line)
                },
                maxPoinCount: 200
                );

            SelectedControlType = ColtrolTypeList[0];

            if (!BootStrapper.GpibController.IsLakeShoreConnected())
                return;

            double tmpChamberTemperature = BootStrapper.GpibController.GetChamberTemperature();
            CurrentTemperature = tmpChamberTemperature.ToString("#.00");
            double tmpSampleTemperature = BootStrapper.GpibController.GetSampleTemperature();
            CurrentSampleTemperature = tmpSampleTemperature.ToString("#.00");
            double tmpTargetTemperature = BootStrapper.GpibController.GetTargetTemperature();
            TargetTemperature = tmpTargetTemperature.ToString("#.00");

            LakeShore.TemperatureUnit unit = BootStrapper.GpibController.GetCurrentLakeShoreTemperatureUnit();
            switch(unit)
            {
                case LakeShore.TemperatureUnit.Celcius:
                    TemperatureUnit = "Celsjusz";
                    break;
                case LakeShore.TemperatureUnit.Kelvins:
                    TemperatureUnit = "Kelwin";
                    break;
            }

            LakeShore.ControlType control = BootStrapper.GpibController.GetCurrentLakeShoreControlType();
            switch(control)
            {
                case LakeShore.ControlType.Manual:
                    PidControlType = "Ręczny PID";
                    break;
                case LakeShore.ControlType.AutoP:
                    PidControlType = "Auto P";
                    break;
                case LakeShore.ControlType.AutoPI:
                    PidControlType = "Auto PI";
                    break;
                case LakeShore.ControlType.AutoPID:
                    PidControlType = "Auto PID";
                    break;
            }

            LakeShorePidValue tmpPid = BootStrapper.GpibController.GetCurrentLakeShorePidValue();
            ParamterPValue = tmpPid.ParamP.ToString();
            ParamterIValue = tmpPid.ParamI.ToString();
            ParamterDValue = tmpPid.ParamD.ToString();
        }

        // Pola
        // --------------------------------------------------

        [ObservableProperty]
        private RealTimePlotModel plotModel;

        [ObservableProperty]
        public List<string> pointCountList = new List<string>()
        {
            "20",
            "50",
            "100",
            "200",
            "500",
            "1000",
            "5000",
            "10000",
            "50000",
            "100000",
            "500000",
            "1000000"
        };
        public string SelectedPointCount
        {
            get => selectedPointCount;
            set
            {
                int newMaxPointCount = int.Parse(value);
                PlotModel.MaxPointCount = newMaxPointCount;
                SetProperty(ref selectedPointCount, value);
            }
        }
        private string selectedPointCount = "1000";

        [ObservableProperty]
        private string currentTemperature = "Nieznana";
        [ObservableProperty]
        private string currentSampleTemperature = "Nieznana";
        [ObservableProperty]
        private string targetTemperature = "Nieznana";
        [ObservableProperty]
        private string temperatureUnit = "Nieznana";
        [ObservableProperty]
        private string paramterPValue = "Nieznana";
        [ObservableProperty]
        private string paramterIValue = "Nieznana";
        [ObservableProperty]
        private string paramterDValue = "Nieznana";
        [ObservableProperty]
        private string pidControlType = "Nieznany";
        [ObservableProperty]
        private string flowValue = "Nieznany";

        [ObservableProperty]
        private List<string> coltrolTypeList = new List<string>
        {
            "Ręczny PID",
            "Auto P",
            "Auto PI",
            "Auto PID",
        };
        private Dictionary<string, LakeShore.ControlType> controlTypeMap = new Dictionary<string, LakeShore.ControlType>
        {
            ["Ręczny PID"] = LakeShore.ControlType.Manual,
            ["Auto P"] = LakeShore.ControlType.AutoP,
            ["Auto PI"] = LakeShore.ControlType.AutoPI,
            ["Auto PID"] = LakeShore.ControlType.AutoPID,
        };
        [ObservableProperty]
        private string selectedControlType = "";

        [ObservableProperty]
        private string forcedTargetTemperatureValue = "";
        [ObservableProperty]
        private bool forceTargetTemperature = false;
        [ObservableProperty]
        private string forcedParamterPValue = "";
        [ObservableProperty]
        private bool forcePid = false;
        [ObservableProperty]
        private string forcedParamterIValue = "";
        [ObservableProperty]
        private string forcedParamterDValue = "";

        public ICommand ForceParametersCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        /// <summary>
        /// Funkcja wymuszająca nastawy sprzętu
        /// </summary>
        private void ForceParameters()
        {
            // Kontrola wartości następuje automatycznie w CanForceParameters
            if (ForceTargetTemperature)
            {
                BootStrapper.GpibController.PushCommand(
                    GpibCommands.SetTargetTemperature,
                    new List<object>
                    {
                        NumericConverters.StringToNumber(ForcedTargetTemperatureValue, ConvertableNumericTypes.Double)!
                    });
            }
            if (ForcePid)
            {
                // Wyłączanie autopid jeżeli jest włączony
                BootStrapper.GpibController.PushCommand(GpibCommands.SetAutoPid);

                // Ustawianie trybu kontroli
                BootStrapper.GpibController.PushCommand(
                    GpibCommands.SetLakeShoreControlMode,
                    new List<object>
                    {
                        controlTypeMap[SelectedControlType]
                    });

                if (controlTypeMap[SelectedControlType] == LakeShore.ControlType.Manual)
                    BootStrapper.GpibController.PushCommand(
                        GpibCommands.SetPid,
                        new List<object>
                        {
                            new LakeShorePidValue(
                                (ushort)NumericConverters.StringToNumber(ForcedParamterPValue, ConvertableNumericTypes.UShort)!,
                                (ushort)NumericConverters.StringToNumber(ForcedParamterIValue, ConvertableNumericTypes.UShort)!,
                                (ushort)NumericConverters.StringToNumber(ForcedParamterDValue, ConvertableNumericTypes.UShort)!
                                )
                        });
            }
        }

        /// <summary>
        /// Funkcja sprawdzająca czy spełniono wymagania wywołania wymuszenia parametrów
        /// </summary>
        /// <returns></returns>
        private bool CanForceParameters()
        {
            // Sprawdzenie czy są parametry do wysłania
            if (!ForceTargetTemperature &&
                !ForcePid
                )
                return false;

            // Sprawdzanie konwertowalności typu
            if (ForceTargetTemperature)
                if (NumericConverters.StringToNumber(ForcedTargetTemperatureValue, ConvertableNumericTypes.Double) == null)
                    return false;
            if (ForcePid && controlTypeMap[SelectedControlType] == LakeShore.ControlType.Manual)
            {
                if (NumericConverters.StringToNumber(ForcedParamterPValue, ConvertableNumericTypes.UShort) == null)
                    return false;
                if (NumericConverters.StringToNumber(ForcedParamterIValue, ConvertableNumericTypes.UShort) == null)
                    return false;
                if (NumericConverters.StringToNumber(ForcedParamterDValue, ConvertableNumericTypes.UShort) == null)
                    return false;
            }

            return true;
        }

        // Funkcje eventów
        // --------------------------------------------------

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnej temperaturze układu.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCurrentTemperatureInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                double tmp = BootStrapper.GpibController.GetChamberTemperature();
                CurrentTemperature = tmp.ToString("#.00");
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnej temperaturze próbki.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCurrentSampleTemperatureInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                double tmp = BootStrapper.GpibController.GetSampleTemperature();
                CurrentSampleTemperature = tmp.ToString("#.00");
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnej jednostce temperatury używanej przez sterownik LakeShore.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTemperatureUnitInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                LakeShore.TemperatureUnit unit = BootStrapper.GpibController.GetCurrentLakeShoreTemperatureUnit();
                switch (unit)
                {
                    case LakeShore.TemperatureUnit.Celcius:
                        TemperatureUnit = "Calsjusz";
                        break;
                    case LakeShore.TemperatureUnit.Kelvins:
                        TemperatureUnit = "Kelwin";
                        break;
                }
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnej nastawie temperatury.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTargetTemperatureInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                double tmp = BootStrapper.GpibController.GetTargetTemperature();
                TargetTemperature = tmp.ToString("#.00");
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnych nastawach PID.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePidInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                LakeShorePidValue tmp = BootStrapper.GpibController.GetCurrentLakeShorePidValue();
                ParamterPValue = tmp.ParamP.ToString();
                ParamterIValue = tmp.ParamI.ToString();
                ParamterDValue = tmp.ParamD.ToString();
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnych nastawach PID.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePidTypeInfo(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                LakeShore.ControlType control = BootStrapper.GpibController.GetCurrentLakeShoreControlType();
                switch (control)
                {
                    case LakeShore.ControlType.Manual:
                        PidControlType = "Ręczny PID";
                        break;
                    case LakeShore.ControlType.AutoP:
                        PidControlType = "Auto P";
                        break;
                    case LakeShore.ControlType.AutoPI:
                        PidControlType = "Auto PI";
                        break;
                    case LakeShore.ControlType.AutoPID:
                        PidControlType = "Auto PID";
                        break;
                }
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informacje o temperaturach do wykresu.
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        private void PlotNewPoint(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                double tmpCurrent = BootStrapper.GpibController.GetChamberTemperature();
                double tmpTarget = BootStrapper.GpibController.GetTargetTemperature();
                DateTime time = DateTime.Now;
                PlotModel.PushNewPoint((int)PlotSeriesID.TargetTemperature, time, tmpTarget);
                PlotModel.PushNewPoint((int)PlotSeriesID.CurrentTemperature, time, tmpCurrent);
            });
        }

        /// <summary>
        /// Funkcja eventu zaciągająca informację o obecnym przepływie
        /// Może być bezpiecznie wywoływana przez eventy z innych wątków.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCurrentFlow(object? sender, System.EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                FlowValue = BootStrapper.FlowSensorController.GetCurrentFlow().ToString();
            });
        }

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Funkcja nadzorująca blokadę przycisku wymuszenia parametrów
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForceParametersCommandLockDedection(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ForceTargetTemperature) ||
                e.PropertyName == nameof(ForcedTargetTemperatureValue) ||
                e.PropertyName == nameof(ForcePid) ||
                e.PropertyName == nameof(ForcedParamterPValue) ||
                e.PropertyName == nameof(ForcedParamterIValue) ||
                e.PropertyName == nameof(ForcedParamterDValue) ||
                e.PropertyName == nameof(SelectedControlType)
                )
            {
                (ForceParametersCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Zmienna identyfiukująca serie danych na wykresie
        /// </summary>
        private enum PlotSeriesID : int
        {
            TargetTemperature,
            CurrentTemperature
        }

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            BootStrapper.GpibController.NewChamberTemperature -= UpdateCurrentTemperatureInfo;
            BootStrapper.GpibController.NewSampleTemperature -= UpdateCurrentSampleTemperatureInfo;
            BootStrapper.GpibController.NewLakeShoreTemperatureUnit -= UpdateTemperatureUnitInfo;
            BootStrapper.GpibController.NewChamberTemperature -= PlotNewPoint;
            BootStrapper.GpibController.NewTargetTemperaturePointEvent -= UpdateTargetTemperatureInfo;
            BootStrapper.GpibController.NewTargetTemperaturePointEvent -= PlotNewPoint;
            BootStrapper.GpibController.NewLakeShorePIDValueEvent -= UpdatePidInfo;
            BootStrapper.GpibController.NewLakeShoreControlType -= UpdatePidTypeInfo;

            BootStrapper.FlowSensorController.NewCurrentFlowEvent -= NewCurrentFlow;

            this.PropertyChanged -= ForceParametersCommandLockDedection;
        }
    }
}
