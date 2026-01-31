using LabServices.DataTemplates;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CultureInfo = System.Globalization.CultureInfo;

namespace LabServices.GpibHardware
{
    public partial class GpibHardwareController : HardwareController
    {
        // Zmienne pętli głównej. Nie powinny być używane pozanią lub komendami przez nią wywoływanymi
        // --------------------------------------------------

        /// <summary>Obiekt kontrolera magistrali Gpib</summary>
        private GpibController _gpib;
        /// <summary>Okres wykonania jednego obiegu pętli kontrolera</summary>
        private long _enginePeriod = 1000;
        /// <summary>Skumulowane opóźnienie pętli głównej w ms</summary>
        private long _engineLag = 0;
        /// <summary>Ustawienia wywołania kontrolera</summary>
        private GpibHardwareInitData _initData;
        /// <summary>Pula wartości automatycznych pid. Null gdy auto pid nieaktywny</summary>
        private LakeShoreAutoPidPool? _autoPidPool = null;
        /// <summary>Parametry wykonywania pomiarów</summary>
        private KithleySweeperInitData? _sweeperInitData = null;

        public GpibHardwareController()
        {
            _gpib = new GpibController();
        }


        protected override void Init(object param)
        {
            // Wczytywanie ustawień
            _initData = (GpibHardwareInitData)param;

            // Inicjalizacja magistrali
            _gpib.Start();

            // Ustawianie flag stanu
            _isLakeShoreConnected.Set(_initData.LakeShoreConnected);
            _isKithleyConnected.Set(_initData.KithleyConnected);

            // Ustawianie bezpiecznego ustalonego stanu LakeShore
            if (_initData.LakeShoreConnected)
            {
                _gpib.DeviceConnect(_initData.LakeShoreAddress);
                _gpib.Write("CUNI C");
                _gpib.Write("SUNI C");
                _gpib.Write("TUNE 3");
                _gpib.Write("SETP 15");
                _gpib.Write("RANG 0");
                _gpib.DeviceDisconnect();

                SetLakeShoreTemperatureUnit(LakeShore.TemperatureUnit.Celcius);
                SetLakeShoreControlType(LakeShore.ControlType.AutoPID);
                SetTargetTemperature(15.0);
            }

            // Zadanie tworzące testową sinusoidę
            /*Task t = new Task(async () =>
            {
                await Task.Delay(500);

                _isLakeShoreConnected.Set(_initData.LakeShoreConnected);
                this.SetLakeShorePidValue(new LakeShorePidValue(1, 2, 3));

                int sampleRate = 5000;
                double amplitude = 200;
                double frequency = 60;
                uint n = 0;

                while (true)
                {
                    await Task.Delay(200);
                    n++;
                    int temp1 = (short)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / sampleRate));
                    int temp2 = (short)(amplitude * Math.Sin((Math.PI * n * frequency) / sampleRate));
                    this.SetCurrentTemperature(temp1);
                    this.SetTargetTemperature(temp2);
                }
            });
            t.Start();*/
        }


        protected override void Finish()
        {
            // Ustawianie flag stanu
            _isLakeShoreConnected.Set(false);
            _isKithleyConnected.Set(false);

            // Ustawianie bezpiecznej nastawy temperatury
            if (_initData.LakeShoreConnected)
            {
                _gpib.DeviceConnect(_initData.LakeShoreAddress);
                _gpib.Write("SETP 15");
                _gpib.DeviceDisconnect();
            }

            // Zamykanie magistrali
            _gpib.Dispose();
        }


        protected override void LoopInteration()
        {
            // Tworzenie licznika odmierzającego czas
            Stopwatch loopWatch = Stopwatch.StartNew();

            if (_initData.LakeShoreConnected)
            {
                _gpib.DeviceConnect(_initData.LakeShoreAddress);

                // Obsługa odczytu wartości temperatury
                string sampleTemperatureResponse = _gpib.Query("SDAT?");
                SetSampleTemperature(double.Parse(sampleTemperatureResponse, CultureInfo.InvariantCulture));

                string chamberTemperatureResponse = _gpib.Query("CDAT?");
                SetChamberTemperature(double.Parse(chamberTemperatureResponse, CultureInfo.InvariantCulture));


                // Obsługa auto pid
                if (_autoPidPool != null && _lakeShoreControlType.Get() == LakeShore.ControlType.Manual)
                {
                    LakeShorePidValue newPidValue = _autoPidPool.GetValue(GetTargetTemperature());
                    LakeShorePidValue oldPidValue = GetCurrentLakeShorePidValue();
                    if (!newPidValue.Equals(oldPidValue))
                    {
                        _gpib.Write($"GAIN {newPidValue.ParamP}");
                        _gpib.Write($"RSET {newPidValue.ParamI}");
                        _gpib.Write($"RATE {newPidValue.ParamD}");
                        SetLakeShorePidValue(newPidValue);
                    }
                }

                // Odczyt pid przy kontroli automatycznej
                if (_lakeShoreControlType.Get() != LakeShore.ControlType.Manual)
                {
                    ushort p = ushort.Parse(_gpib.Query("GAIN?"), CultureInfo.InvariantCulture);
                    ushort i = ushort.Parse(_gpib.Query("RSET?"), CultureInfo.InvariantCulture);
                    ushort d = ushort.Parse(_gpib.Query("RATE?"), CultureInfo.InvariantCulture);
                    SetLakeShorePidValue(new LakeShorePidValue(p, i, d));
                }

                _gpib.DeviceDisconnect();
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
                    Log.Information($"GpibController-Executed:{commandData.CommandNumber}");
                    _commandPool.ExecuteCommand(commandData.CommandNumber, commandData.ParamList);
                }
            }

            // Wykrycie zgubienia przebiegu pętli
            if (_engineLag > _enginePeriod)
            {
                long lostPeriods = _engineLag / _enginePeriod;
                long totalLostTime = _engineLag - (_engineLag % _enginePeriod);
                _engineLag = _engineLag % _enginePeriod;
                Log.Error($"GpibController-LoopInteration LostPeriods:{lostPeriods},TotalLostTime:{totalLostTime}ms");
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

        /// <summary>
        /// Funkcja wywołująca zapytanie do kithley w celu podtrzymania połączenia
        /// </summary>
        private void MessageKithley()
        {
            _gpib.DeviceConnect(_initData.KithleyAddress);
            string result = _gpib.Query("*IDN?");
            _gpib.DeviceDisconnect();
        }
    }
}
