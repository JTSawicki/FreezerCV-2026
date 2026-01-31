using LabServices.DataTemplates;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    public enum GpibCommands : ushort
    {
        /// <summary>
        /// Ta komenda nic nie robi 😛
        /// Domyślnie: brak
        /// Ograniczenie: brak
        /// Parametry: brak
        /// </summary>
        DummyCommand,
        /// <summary>
        /// Komenda wywołuje sweep
        /// Parametry: KithleySweeperInitData
        /// </summary>
        Sweep,
        /// <summary>
        /// Nastawia parametry sweepera.
        /// Parametry: KithleySweeperInitData
        /// </summary>
        SetSweeperParameters,
        /// <summary>
        /// Nastawia wartość PID sterownika Lumel
        /// Parametry: LakeShorePidValue
        /// </summary>
        SetPid,
        /// <summary>
        /// Nastawia docelową temperaturę sterownika Lumel
        /// Ograniczenie: Patrz dokumentacja Lumel Re72
        /// Parametry: double(temperatura)
        /// </summary>
        SetTargetTemperature,
        /// <summary>
        /// Zmienia wartości nastawy temperatury o zadaną wartość
        /// Parametry: double(zmiana może być ujemna)
        /// </summary>
        ChangeTargetTemperature,
        /// <summary>
        /// Ustawia auto pid
        /// Ograniczenie: Pojedyńcza nastawa w zakresie 0 - 9999
        /// Parametry: LakeShoreAutoPidPool
        /// </summary>
        SetAutoPid,
        /// <summary>
        /// Ustawia tryb pracy LakeShore
        /// Parametry: LakeShore.ControlType
        /// </summary>
        SetLakeShoreControlMode,
        /// <summary>
        /// Ustawia jednostkę temperatury LakeShore
        /// Parametry: LakeShore.TemperatureUnit
        /// </summary>
        SetLakeShoreTemperatureUnit,
    }

    public partial class GpibHardwareController : HardwareController
    {
        protected override void RegisterCommands()
        {
            _commandPool.RegisterCommand((ushort)GpibCommands.DummyCommand, DummyCommand);
            _commandPool.RegisterCommand((ushort)GpibCommands.Sweep, Sweep);
            _commandPool.RegisterCommand((ushort)GpibCommands.SetSweeperParameters, SetSweeperParameters);
            _commandPool.RegisterCommand((ushort)GpibCommands.SetPid, SetPid);
            _commandPool.RegisterCommand((ushort)GpibCommands.SetTargetTemperature, SetTargetTemperature);
            _commandPool.RegisterCommand((ushort)GpibCommands.ChangeTargetTemperature, ChangeTargetTemperature);
            _commandPool.RegisterCommand((ushort)GpibCommands.SetAutoPid, SetAutoPid);

            _commandPool.RegisterCommand((ushort)GpibCommands.SetLakeShoreControlMode, SetLakeShoreControlMode);
            _commandPool.RegisterCommand((ushort)GpibCommands.SetLakeShoreTemperatureUnit, SetLakeShoreTemperatureUnit);
        }

        private void DummyCommand(List<object> param)
        {
            ;
        }

        private void SetSweeperParameters(List<object> param)
        {
            // Sprawdzenie poprawności danych wejściowych
            if (param.Count != 1 ||
                param[0] is not KithleySweeperInitData)
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetSweeperParameters");
                return;
            }

            _sweeperInitData = (KithleySweeperInitData)param[0];
        }

        private void Sweep(List<object> param)
        {
            // Sprawdzenie czy można wykonać komendę
            if (!_initData.KithleyConnected)
                return;

            // Sprawdzenie czy wykonano inicjalizację parametrów pomiaru
            if (!_sweeperInitData.HasValue)
            {
                Log.Error($"Kithley sweep data is not initialized GpibHardwareController.Sweep");
                return;
            }

            KithleySweeperInitData data = _sweeperInitData.Value;

            // Nawiązywanie połączenia z kithley
            _gpib.DeviceConnect(_initData.KithleyAddress);

            // Inicjalizacja zmiennych pomocniczych
            string output = "";
            int numberOfPoints;

            // Inicjalizacja sweep
            _gpib.Write("*RST");
            _gpib.Write("*CLS");

            _gpib.Write("SENS:FUNC:CONC OFF");
            _gpib.Write("SOUR:FUNC VOLT");
            _gpib.Write("SENS:FUNC 'CURR:DC'");
            _gpib.Write($"SENS:CURR:PROT {data.CurrentProtection}");

            // Podłączenie 2 lub 4 kable
            if (data.ConnectionType == Kithley.ConnectionType.TwoWireTerminal)
                _gpib.Write("SYST:RSEN OFF");
            else if (data.ConnectionType == Kithley.ConnectionType.FourWireTerminal)
                _gpib.Write("SYST:RSEN ON");
            else
            {
                Log.Error($"Not supported connection type: {Enum.GetName(typeof(Kithley.ConnectionType), data.ConnectionType)}");
                return;
            }

            // Tryb segmentacji danych
            if (data.VoltageSegmentation == Kithley.VoltageSegmentation.Linear)
            {
                _gpib.Write("SOUR:SWE:SPAC LIN");
                _gpib.Write($"SOUR:VOLT:START {data.VoltageStart}");
                _gpib.Write($"SOUR:VOLT:STOP {data.VoltageStop}");
                _gpib.Write($"SOUR:VOLT:STEP {data.VoltageStep}");

                numberOfPoints = int.Parse(_gpib.Query("SOUR:SWE:POIN?"));

                _gpib.Write("SOUR:VOLT:MODE SWE");
                _gpib.Write("SOUR:SWE:DIR UP");

                _gpib.Write($"TRIG:COUN {numberOfPoints}");
            }
            else if (data.VoltageSegmentation == Kithley.VoltageSegmentation.Logarytmic)
            {
                _gpib.Write("SOUR:SWE:SPAC LOG");
                _gpib.Write($"SOUR:VOLT:START {data.VoltageStart}");
                _gpib.Write($"SOUR:VOLT:STOP {data.VoltageStop}");

                numberOfPoints = data.VoltagePoints;
                _gpib.Write($"SOUR:SWE:POIN {numberOfPoints}");

                _gpib.Write("SOUR:VOLT:MODE SWE");
                _gpib.Write("SOUR:SWE:DIR UP");

                _gpib.Write($"TRIG:COUN {numberOfPoints}");

            }
            else
            {
                Log.Error($"Not supported voltage segmentation: {Enum.GetName(typeof(Kithley.VoltageSegmentation), data.VoltageSegmentation)}");
                return;
            }

            _gpib.Write($"SOUR:DEL {data.SourceDelay.ToString("#.0000", System.Globalization.CultureInfo.InvariantCulture)}");
            _gpib.Write("FORM:ELEM VOLT,CURR");

            // Aktywacja eventu srq
            //_gpib.Write("STAT:OPER:ENAB 1024");
            //_gpib.Write("*SRE 128");
            //_gpib.EnableSRQ();

            // Wykonanie sweep
            try
            {
                _gpib.Write("OUTPUT ON");
                _gpib.Write("INIT");
                _gpib.Write("*OPC");
                // _gpib.WaitForSRQ((data.SourceDelay + 5000) * numberOfPoints);
                Thread.Sleep(((int)(data.SourceDelay * 1000 + 100)) * numberOfPoints * 2);
                output = _gpib.QueryBigResponse("FETCH?", (numberOfPoints * 2 * 15) + 10);
            }
            finally
            {
                _gpib.Write("OUTPUT OFF");
            }

            // Powrót do połączenia z LakeShore
            _gpib.DeviceDisconnect();

            // Zwracanie wyniku działania
            // Opakowano w Task w celu zwiększenia responsywności czasowej pętli głównej
            Task t = new Task(ParseKithleyOutput, output);
            t.Start();
        }

        /// <summary>
        /// Funkcja parsuje i wysyła dane do 
        /// </summary>
        /// <param name="input">Zwrócony string</param>
        private void ParseKithleyOutput(object? input)
        {
            // Konwersja danych
            if (input == null || input is not string)
                return;
            string data = (string)input;

            // Buforowanie danych w celu maksymalnej dokładności
            DateTime timestamp = DateTime.Now;
            double temperature = this.GetChamberTemperature();

            // Prasowanie
            string[] splited = data.Split(',');

            double[] voltage = new double[splited.Length / 2];
            double[] current = new double[splited.Length / 2];
            double[] resistance = new double[splited.Length / 2];

            for (int i = 0; i < splited.Length; i++)
            {
                if (i % 2 == 0)
                    voltage[i / 2] = double.Parse(splited[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                else
                    current[i / 2] = double.Parse(splited[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            }
            for (int i = 0; i < voltage.Length; i++)
            {
                resistance[i] = voltage[i] / current[i];
            }

            KithleyMeasurement measurement = new KithleyMeasurement()
            {
                Voltage = voltage,
                Current = current,
                Resistance = resistance,
                TimeStamp = timestamp,
                Length = voltage.Length,
                Temperature = temperature
            };
            this.PushNewMeasurement(measurement);
        }

        private void SetPid(List<object> param)
        {
            if (!_initData.LakeShoreConnected)
                return;

            if (_autoPidPool != null)
            {
                Log.Information("Skiping pid set because auto pid is active in GpibHardwareController.SetPid");
                return;
            }
            if (param.Count != 1 ||
                param[0] is not LakeShorePidValue)
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetPid");
                return;
            }

            if (_lakeShoreControlType.Get() != LakeShore.ControlType.Manual)
            {
                string controlTypeName = Enum.GetName(typeof(LakeShore.ControlType), _lakeShoreControlType.Get()) ?? "Error";
                Log.Warning($"In GpibHardwareController.SetPid: Tried to set pid value while ControlType == {controlTypeName}");
                return;
            }

            LakeShorePidValue newPidValue = (LakeShorePidValue)param[0];
            _gpib.DeviceConnect(_initData.LakeShoreAddress);
            _gpib.Write($"GAIN {newPidValue.ParamP}");
            _gpib.Write($"RSET {newPidValue.ParamI}");
            _gpib.Write($"RATE {newPidValue.ParamD}");
            _gpib.DeviceDisconnect();
            SetLakeShorePidValue(newPidValue);
        }

        private void SetTargetTemperature(List<object> param)
        {
            if (!_initData.LakeShoreConnected)
                return;

            if (param.Count != 1 ||
                param[0] is not double)
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetTargetTemperature");
                return;
            }

            _gpib.DeviceConnect(_initData.LakeShoreAddress);
            _gpib.Write($"SETP " + ((double)param[0]).ToString("#.00"));
            _gpib.DeviceDisconnect();
            SetTargetTemperature((double)param[0]);
        }

        private void ChangeTargetTemperature(List<object> param)
        {
            if (!_initData.LakeShoreConnected)
                return;

            if (param.Count != 1 ||
                param[0] is not double)
            {
                Log.Error($"Bad parameter in GpibHardwareController.ChangeTargetTemperature");
                return;
            }

            double newTarget = this.GetTargetTemperature() + (double)param[0];
            _gpib.DeviceConnect(_initData.LakeShoreAddress);
            _gpib.Write($"SETP " +  newTarget.ToString("#.00"));
            _gpib.DeviceDisconnect();
            SetTargetTemperature(newTarget);
        }

        private void SetAutoPid(List<object> param)
        {
            if (param.Count > 1 ||
                (param.Count == 1 && param[0] is not LakeShoreAutoPidPool))
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetAutoPid");
                return;
            }
            if (param.Count == 0)
                _autoPidPool = null;
            else
                _autoPidPool = (LakeShoreAutoPidPool)param[0];
        }

        private void SetLakeShoreControlMode(List<object> param)
        {
            if (!_initData.LakeShoreConnected)
                return;

            if (param.Count != 1 ||
                param[0] is not LakeShore.ControlType)
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetLakeShoreControlMode");
                return;
            }

            _gpib.DeviceConnect(_initData.LakeShoreAddress);
            switch ((LakeShore.ControlType)param[0])
            {
                case LakeShore.ControlType.Manual:
                    _gpib.Write("TUNE 0");
                    break;
                case LakeShore.ControlType.AutoP:
                    _gpib.Write("TUNE 1");
                    break;
                case LakeShore.ControlType.AutoPI:
                    _gpib.Write("TUNE 2");
                    break;
                case LakeShore.ControlType.AutoPID:
                    _gpib.Write("TUNE 3");
                    break;
            }
            _gpib.DeviceDisconnect();
            SetLakeShoreControlType((LakeShore.ControlType)param[0]);
        }

        private void SetLakeShoreTemperatureUnit(List<object> param)
        {
            if (!_initData.LakeShoreConnected)
                return;

            if (param.Count != 1 ||
                param[0] is not LakeShore.TemperatureUnit)
            {
                Log.Error($"Bad parameter in GpibHardwareController.SetLakeShoreTemperatureUnit");
                return;
            }

            _gpib.DeviceConnect(_initData.LakeShoreAddress);
            switch ((LakeShore.TemperatureUnit)param[0])
            {
                case LakeShore.TemperatureUnit.Celcius:
                    _gpib.Write("CUNI C");
                    _gpib.Write("SUNI C");
                    break;
                case LakeShore.TemperatureUnit.Kelvins:
                    _gpib.Write("CUNI K");
                    _gpib.Write("SUNI K");
                    break;
            }
            _gpib.DeviceDisconnect();
            SetLakeShoreTemperatureUnit((LakeShore.TemperatureUnit)param[0]);
        }
    }
}
