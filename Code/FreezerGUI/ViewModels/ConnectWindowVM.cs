using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabControlsWPF;
using System.Diagnostics;
using OxyPlot;
using FreezerGUI.Windows;
using LabServices.GpibHardware;
using System.IO.Ports;
using System.Threading;
using LabServices.FlowSensor;

namespace FreezerGUI.ViewModels
{
    /// <summary>
    /// ViewModel dla okna podłączania
    /// </summary>
    public partial class ConnectWindowVM : ObservableObject, IDisposable
    {
        public ConnectWindowVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów

            // Inicjalizowanie komend
            OpenLicenceCommand = new RelayCommand(OpenLicence);
            OpenSettingsCommand = new RelayCommand<Window>(OpenSettings);
            ConnectCommand = new RelayCommand<Window>(Connect);
        }

        public void InitializeDefaultValues()
        {
            SelectedKithleyAdress = BootStrapper.Settings.DefaultKithleyAdress;
            IsKithleyConnected = false;
            SelectedLakeShoreAdress = BootStrapper.Settings.DefaultLakeShoreAdress;
            IsLakeShoreConnected = false;

            ComList = new List<string>(SerialPort.GetPortNames());
            ComList.Insert(0, "auto");
            SelectedFlowSensorCom = ComList[0];
            IsFlowSensorConnected = false;
        }

        // Pola
        // --------------------------------------------------

        public IList<string> KithleyAdressList => Enumerable.Range(Constants.GpibMinAddress, Constants.GpibMaxAddress)
            .Select(x => x.ToString())
            .Where(x => !x.Equals(SelectedLakeShoreAdress) && !x.Equals(Constants.GpibControllerAddress.ToString()))
            .ToList();
        [ObservableProperty, NotifyPropertyChangedFor(nameof(LakeShoreAdressList))]
        private string selectedKithleyAdress;
        [ObservableProperty]
        private bool isKithleyConnected;

        public IList<string> LakeShoreAdressList => Enumerable.Range(Constants.GpibMinAddress, Constants.GpibMaxAddress)
            .Select(x => x.ToString())
            .Where(x => !x.Equals(SelectedKithleyAdress) && !x.Equals(Constants.GpibControllerAddress.ToString()))
            .ToList();
        [ObservableProperty, NotifyPropertyChangedFor(nameof(KithleyAdressList))]
        private string selectedLakeShoreAdress;
        [ObservableProperty]
        private bool isLakeShoreConnected;

        [ObservableProperty]
        private List<string> comList;
        [ObservableProperty]
        private string selectedFlowSensorCom;
        [ObservableProperty]
        private bool isFlowSensorConnected;

        public RelayCommand OpenLicenceCommand { get; }
        public RelayCommand<Window> OpenSettingsCommand { get; }
        public RelayCommand<Window> ConnectCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        /// <summary>
        /// 
        /// </summary>
        private void OpenLicence()
        {
            Windows.LicenceWindow licenceWindow = new Windows.LicenceWindow();
            licenceWindow.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OpenSettings(Window? window)
        {
            Windows.SettingsWindow settingsWindow = new Windows.SettingsWindow();
            settingsWindow.Show();
            window!.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Connect(Window? window)
        {
            if (!CheckExistanceOfGpibHardware())
                return;

            if (IsLakeShoreConnected || IsKithleyConnected)
            {
                // Kontroler Gpib
                // ----------------------------------------------------------------------
                GpibHardwareInitData initData = new GpibHardwareInitData()
                {
                    KithleyAddress = int.Parse(SelectedKithleyAdress),
                    KithleyConnected = IsKithleyConnected,
                    LakeShoreAddress = int.Parse(SelectedLakeShoreAdress),
                    LakeShoreConnected = IsLakeShoreConnected,
                };
                BootStrapper.GpibController.StartController(initData);
            }
            
            // Kontroler czujnika przepływu
            // ----------------------------------------------------------------------
            if (IsFlowSensorConnected)
            {
                string? port = SelectedFlowSensorCom;
                if (port.Equals("auto"))
                {
                    port = FindFlowSensorPort();
                    if (port == null)
                    {
                        MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, $"Nie podłączono czujnika przepływu");
                        return;
                    }
                }
                else
                {
                    if (!CheckFlowSensorPort(port))
                    {
                        MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, $"Nie podłączono czujnika do wybranego portu");
                        return;
                    }
                }

                BootStrapper.FlowSensorController.StartController(port);
            }

            // Przerwa aby dać kontrolerom czas na włączenie się
            // (Zapobiega zbędnym błędom)
            // ----------------------------------------------------------------------
            Thread.Sleep(10);

            // Otwarcie okna main
            // ----------------------------------------------------------------------
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            window!.Close();
        }

        // Funkcje eventów
        // --------------------------------------------------

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Funkcja znajduje port czujnika przepływu
        /// </summary>
        /// <returns></returns>
        private string? FindFlowSensorPort()
        {
            List<string> comList = new List<string>(SerialPort.GetPortNames());

            foreach (string com in comList)
            {
                SerialPort _serial = new SerialPort(com, FlowSensor.Baudrate);
                _serial.Open();
                _serial.WriteLine("01");

                Stopwatch sw = Stopwatch.StartNew();
                string response = string.Empty;
                while (response.Equals(string.Empty) && sw.ElapsedMilliseconds < 100)
                {
                    if (_serial.BytesToRead > 0)
                        response = _serial.ReadLine().Trim();
                }
                sw.Stop();
                _serial.Close();

                if (response.Equals(FlowSensor.IdentifyResponse))
                    return com;
            }

            return null;
        }

        /// <summary>
        /// Sprawdza czy czujnik przepływu jest podłączony do podanego portu COM
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        private bool CheckFlowSensorPort(string com)
        {
            SerialPort _serial = new SerialPort(com, FlowSensor.Baudrate);
            _serial.Open();
            _serial.WriteLine("01");

            Stopwatch sw = Stopwatch.StartNew();
            string response = string.Empty;
            while (response.Equals(string.Empty) && sw.ElapsedMilliseconds < 100)
            {
                if (_serial.BytesToRead > 0)
                    response = _serial.ReadLine().Trim();
            }
            sw.Stop();
            _serial.Close();

            if (response.Equals(FlowSensor.IdentifyResponse))
                return true;
            return false;
        }

        /// <summary>
        /// Sprawdza czy podpięto kontroler GPIB i wszystkie potrzebne urządzenia
        /// </summary>
        /// <returns>Czy podpięto</returns>
        private bool CheckExistanceOfGpibHardware()
        {
            GpibController gpib = new GpibController();

            // Kontroler GPIB
            if (IsKithleyConnected || IsLakeShoreConnected)
            {
                try
                {
                    gpib.Start();
                }
                catch
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, "Nie podpięto kontrolera GPIB");
                    return false;
                }
            }

            // Kithley 2400
            if (IsKithleyConnected)
            {
                try
                {
                    gpib.DeviceConnect(int.Parse(SelectedKithleyAdress));
                    gpib.Query("*IDN?");
                    gpib.DeviceDisconnect();
                }
                catch
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, $"Niepoprawny adres ({SelectedKithleyAdress}) lub niepodpięty miernik Kithley 2400");
                    gpib.Dispose();
                    return false;
                }
            }

            // LakeShore 330
            if (IsLakeShoreConnected)
            {
                try
                {
                    gpib.DeviceConnect(int.Parse(SelectedLakeShoreAdress));
                    gpib.Query("*IDN?");
                    gpib.DeviceDisconnect();
                }
                catch
                {
                    MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, $"Niepoprawny adres ({SelectedLakeShoreAdress}) lub niepodpięty miernik LakeShore 330");
                    gpib.Dispose();
                    return false;
                }
            }

            gpib.Dispose();
            return true;
        }

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            ;
        }
    }
}
