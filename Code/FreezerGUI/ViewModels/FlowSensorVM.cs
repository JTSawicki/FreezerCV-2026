using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreezerM;
using LabControlsWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowSensorCommands = LabServices.FlowSensor.FlowSensorCommands;

namespace FreezerGUI.ViewModels
{
    public partial class FlowSensorVM : ObservableObject, IDisposable
    {
        public FlowSensorVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów
            BootStrapper.FlowSensorController.NewCurrentFlowEvent += NewCurrentFlowEvent;
            BootStrapper.FlowSensorController.NewTargetFlowEvent += NewTargetFlowEvent;
            BootStrapper.FlowSensorController.NewSensorStateEvent += NewSensorStateEvent;

            // Inicjalizowanie komend
            SendTargetFlowCommand = new RelayCommand(SendTargetFlow);
            SendWiFiSettingsCommand = new RelayCommand(SendWiFiSettings);
            SendEMailSettingsCommand = new RelayCommand(SendEMailSettings);
            ArmSensorCommand = new RelayCommand(ArmSensor);
            DisarmSensorCommand = new RelayCommand(DisarmSensor);
            ReconnectWiFiSensorCommand = new RelayCommand(ReconnectWiFiSensor);
            SendTestEMailCommand = new RelayCommand(SendTestEMail);
        }

        public void InitializeDefaultValues()
        {
            ;
        }

        // Pola
        // --------------------------------------------------
        [ObservableProperty]
        private string realFlowTargetInfo = "Nieznana";
        [ObservableProperty]
        private string realFlowCurrentInfo = "Nieznana";

        [ObservableProperty]
        private bool? sensorArmed = null;
        [ObservableProperty]
        private bool? sensorAlarmTrigerd = null;
        [ObservableProperty]
        private bool? sensorConnectedToWiFi = null;

        [ObservableProperty]
        private string selectedFlowTarget = "";

        [ObservableProperty]
        private string selectedWiFiName = "";
        [ObservableProperty]
        private string selectedWiFiPassword = "";

        [ObservableProperty]
        private string selectedServer = "";
        [ObservableProperty]
        private string selectedServerPort = "";
        [ObservableProperty]
        private string selectedEMailLogin = "";
        [ObservableProperty]
        private string selectedEMailPassword = "";

        public RelayCommand SendTargetFlowCommand { get; }
        public RelayCommand SendWiFiSettingsCommand { get; }
        public RelayCommand SendEMailSettingsCommand { get; }
        public RelayCommand SendTestEMailCommand { get; }

        public RelayCommand ArmSensorCommand { get; }
        public RelayCommand DisarmSensorCommand { get; }
        public RelayCommand ReconnectWiFiSensorCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        public void ArmSensor()
        {
            BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.Arm);
        }

        public void DisarmSensor()
        {
            BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.Disarm);
        }

        public void ReconnectWiFiSensor()
        {
            BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.ReconnectWiFi);
        }

        public void SendTargetFlow()
        {
            try
            {
                object? target = NumericConverters.StringToNumber(SelectedFlowTarget, ConvertableNumericTypes.Double);
                BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.SetTargetFlow, new List<object> { target! });
            } catch { }
        }

        public void SendWiFiSettings()
        {
            BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.SetTargetFlow, new List<object>
            {
                SelectedWiFiName,
                SelectedWiFiPassword
            });
        }

        public void SendEMailSettings()
        {
            try
            {
                BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.SetTargetFlow, new List<object>
                {
                    SelectedServer,
                    NumericConverters.StringToNumber(SelectedServerPort, ConvertableNumericTypes.Int)!,
                    SelectedEMailLogin,
                    SelectedEMailPassword
                });
            }
            catch { }
        }

        public void SendTestEMail()
        {
            BootStrapper.FlowSensorController.PushCommand(FlowSensorCommands.SendTestEmail);
        }

        // Funkcje eventów
        // --------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        private void NewCurrentFlowEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                RealFlowCurrentInfo = BootStrapper.FlowSensorController.GetCurrentFlow().ToString();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void NewTargetFlowEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                RealFlowTargetInfo = BootStrapper.FlowSensorController.GetTargetFlow().ToString();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void NewSensorStateEvent(object? sender, EventArgs e)
        {
            BootStrapper.Dispatcher.Invoke(() =>
            {
                SensorArmed = BootStrapper.FlowSensorController.GetSensorArmed();
                SensorAlarmTrigerd = BootStrapper.FlowSensorController.GetSensorAlarm();
                SensorConnectedToWiFi = BootStrapper.FlowSensorController.GetWifiConnected();
            });
        }

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            BootStrapper.FlowSensorController.NewCurrentFlowEvent -= NewCurrentFlowEvent;
            BootStrapper.FlowSensorController.NewTargetFlowEvent -= NewTargetFlowEvent;
            BootStrapper.FlowSensorController.NewSensorStateEvent -= NewSensorStateEvent;
        }
    }
}
