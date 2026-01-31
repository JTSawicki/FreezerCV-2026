using LabServices.DataTemplates;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabServices.FlowSensor
{
    public partial class FlowSensorController : HardwareController
    {
        /// <summary>SerialPort na którym znajduje się czujnik</summary>
        private SerialConteroller _serial;
        /// <summary>Okres wykonania jednego obiegu pętli kontrolera</summary>
        private long _enginePeriod = 100;
        /// <summary>Skumulowane opóźnienie pętli głównej w ms</summary>
        private long _engineLag = 0;

        /// <summary>Co ile okreasów pętli kontrolera odczytać wskazanie temperatury</summary>
        private long _engineReadMultipler = 5;
        /// <summary>Zmienna odliczająca kiedy wykonać odczyt temperatury(okres co _enginePeriod * _engineReadMultipler)</summary>
        private long _engineReadCounter = 0;

        public FlowSensorController()
        {
            ;
        }

        protected override void Init(object param)
        {
            _serial = new SerialConteroller((string)param, FlowSensor.Baudrate, 200, 20);
        }

        protected override void Finish()
        {
            _serial.Dispose();
        }

        protected override void LoopInteration()
        {
            // Tworzenie licznika odmierzającego czas
            Stopwatch loopWatch = Stopwatch.StartNew();

            // Obsługa odczytu parametrów
            _engineReadCounter++;
            if (_engineReadCounter >= _engineReadMultipler)
            {
                _engineReadCounter = 0;
                string? response = _serial.Query("02");
                if (response != null) try
                    {
                        SetCurrentFlow(double.Parse(response, NumberStyles.Float, CultureInfo.InvariantCulture));
                    }
                    catch { }
                response = _serial.Query("03");
                if (response != null) try
                    {
                        SetTargetFlow(double.Parse(response, NumberStyles.Float, CultureInfo.InvariantCulture));
                    }
                    catch { }

                response = _serial.Query("09");
                if (response != null) try
                    {
                        SetSensorArmed(bool.Parse(response));
                    }
                    catch { }
                response = _serial.Query("04");
                if (response != null) try
                    {
                        SetSensorAlarm(bool.Parse(response));
                    }
                    catch { }
                response = _serial.Query("21");
                if (response != null) try
                    {
                        SetWifiConnected(bool.Parse(response));
                    }
                    catch { }
            }

            // Obsługa komend
            while (_controllerCommands.Count > 0)
            {
                // Przerwanie gdy czas wykonania pętli przekracza oczekiwany okres(zwiększa precyzję punktów temperatury)
                if (loopWatch.ElapsedMilliseconds > _enginePeriod)
                    break;
                // Odczyt i wywołanie komendy
                ControllerCommandData commandData;
                bool dequeueFlag = _controllerCommands.TryDequeue(out commandData);
                if (dequeueFlag)
                {
                    Log.Information($"FlowSensorController-Executed:{commandData.CommandNumber}");
                    _commandPool.ExecuteCommand(commandData.CommandNumber, commandData.ParamList);
                }
            }


            // Wykrycie zgubienia przebiegu pętli
            if (_engineLag > _enginePeriod)
            {
                long lostPeriods = _engineLag / _enginePeriod;
                long totalLostTime = _engineLag - (_engineLag % _enginePeriod);
                _engineLag = _engineLag % _enginePeriod;
                Log.Error($"FlowSensorController-LoopInteration LostPeriods:{lostPeriods},TotalLostTime:{totalLostTime}ms");
            }

            // Obsługa przerwania(wykonywane w celu zapewnienia jak największej dokładności okresu wykonania pętli)
            long loopExecutionTime = loopWatch.ElapsedMilliseconds;
            if (loopExecutionTime + _engineLag < _enginePeriod)
            {
                _engineLag = 0;
                Thread.Sleep((int)(_enginePeriod - (loopExecutionTime + _engineLag)));
                return;
            }
            else
            {
                _engineLag = loopExecutionTime + _engineLag - _enginePeriod;
                Thread.Sleep((int)_enginePeriod);
                return;
            }
        }
    }
}
