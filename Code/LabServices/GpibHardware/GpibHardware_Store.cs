using LabServices.DataTemplates;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    public partial class GpibHardwareController : HardwareController
    {
        // Pola LakeShore
        // --------------------------------------------------

        /// <summary>Czy jest aktywnie podpięty kithley</summary>
        private LockedProperty<bool> _isKithleyConnected = new LockedProperty<bool>(false);
        /// <summary>Czy jest aktywnie podpięty lakeshore</summary>
        private LockedProperty<bool> _isLakeShoreConnected = new LockedProperty<bool>(false);

        /// <summary>Obecna nastawa temperatury</summary>
        private LockedProperty<double> _targetTemperature = new LockedProperty<double>(0);
        /// <summary>
        /// Publiczny event wywoływany przy dodaniu nowego punktu nastawy temperatury
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewTargetTemperaturePointEvent;

        /// <summary>Obecna temperatura komory wskazywana przez sterownik LakeShore</summary>
        private LockedProperty<double> _chamberTemperature = new LockedProperty<double>(0);
        /// <summary>
        /// Publiczny event wywoływany przy dodaniu nowego punktu odczytu temperatury komory
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewChamberTemperature;

        /// <summary>Obecna temperatura próbki wskazywana przez sterownik LakeShore</summary>
        private LockedProperty<double> _sampleTemperature = new LockedProperty<double>(0);
        /// <summary>
        /// Publiczny event wywoływany przy dodaniu nowego punktu odczytu temperatury próbki
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewSampleTemperature;

        /// <summary>Obecna wartość PID wskazywana przez sterownik LakeShore</summary>
        private LockedProperty<LakeShorePidValue> _currentLakeShorePidValue = new LockedProperty<LakeShorePidValue>(
            new LakeShorePidValue { ParamP = 0, ParamI = 0, ParamD = 0 }
            );
        /// <summary>
        /// Publiczny event wywoływany przy zmianie parametrów PID sterownika LakeShore
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewLakeShorePIDValueEvent;

        /// <summary>Obecna metoda kontroli nastaw</summary>
        private LockedProperty<LakeShore.ControlType> _lakeShoreControlType = new LockedProperty<LakeShore.ControlType>(LakeShore.ControlType.AutoPID);
        /// <summary>
        /// Publiczny event wywoływany przy zmianie metody kontroli nastaw
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewLakeShoreControlType;

        /// <summary>Obecnie używana jednostka temperatury</summary>
        private LockedProperty<LakeShore.TemperatureUnit> _lakeShoreTemperatureUnit = new LockedProperty<LakeShore.TemperatureUnit>(LakeShore.TemperatureUnit.Celcius);
        /// <summary>
        /// Publiczny event wywoływany przy zmianie jednostki temperatury
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewLakeShoreTemperatureUnit;

        // Pola Kithley
        // --------------------------------------------------

        /// <summary>Lista zawierająca wszystkie pomiary </summary>
        private List<KithleyMeasurement> _measurements = new List<KithleyMeasurement>();
        private object _lockMeasurements = new object();
        /// <summary>
        /// Publiczny event wywoływany przy dodaniu nowego pomiaru oraz zerowaniu buforów
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewMeasuermentEvent;

        /// <summary>Obecnie używane parametry inicjalizacji pomiaru</summary>
        private LockedProperty<KithleySweeperInitData?> _currentKithleyInitData = new LockedProperty<KithleySweeperInitData?>(null);
        /// <summary>
        /// Publiczny event wywoływany przy zmianie parametrów wykonywania pomiarów
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewMeasurementParametersEvent;

        // Funkcje Globalne
        // --------------------------------------------------

        /// <summary>
        /// Czy aktywne jest podpięcie Kithley
        /// </summary>
        /// <returns></returns>
        public bool IsLakeShoreConnected()
        {
            return _isLakeShoreConnected.Get();
        }

        /// <summary>
        /// Czy aktywne jest podpięcie LakeShore
        /// </summary>
        /// <returns></returns>
        public bool IsKithleyConnected()
        {
            return _isKithleyConnected.Get();
        }

        // Funkcje Set
        // --------------------------------------------------

        /// <summary>
        /// Funkcja ustawia punkt nastawy temperatury
        /// </summary>
        /// <param name="temperature"></param>
        internal void SetTargetTemperature(double temperature)
        {
            _targetTemperature.Set(temperature);
            NewTargetTemperaturePointEvent?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funkcja ustawia nowy punkt odczytu temperatury komory
        /// </summary>
        /// <param name="temperature"></param>
        internal void SetChamberTemperature(double temperature)
        {
            _chamberTemperature.Set(temperature);
            NewChamberTemperature?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funkcja ustawia nowy punkt odczytu temperatury próbki
        /// </summary>
        /// <param name="temperature"></param>
        internal void SetSampleTemperature(double temperature)
        {
            _sampleTemperature.Set(temperature);
            NewSampleTemperature?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funkcja ustawia nowe wartości PID
        /// </summary>
        /// <param name="pidValue"></param>
        internal void SetLakeShorePidValue(LakeShorePidValue pidValue)
        {
            _currentLakeShorePidValue.Set(pidValue);
            NewLakeShorePIDValueEvent?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funckja ustawia typ kontroli
        /// </summary>
        /// <param name="controlType"></param>
        public void SetLakeShoreControlType(LakeShore.ControlType controlType)
        {
            _lakeShoreControlType.Set(controlType);
            NewLakeShoreControlType?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Ustawia nową jednostkę temperatury
        /// </summary>
        /// <param name="temperatureUnit"></param>
        public void SetLakeShoreTemperatureUnit(LakeShore.TemperatureUnit temperatureUnit)
        {
            _lakeShoreTemperatureUnit.Set(temperatureUnit);
            NewLakeShoreTemperatureUnit?.Invoke(new object(), EventArgs.Empty);
        }

        // Funkcje Get
        // --------------------------------------------------

        /// <summary>
        /// Funkcja zwraca obecną wartość nastawy temperatury
        /// </summary>
        /// <returns></returns>
        public double GetTargetTemperature()
        {
            return _targetTemperature.Get();
        }

        /// <summary>
        /// Funcja zwraca obecną wartość odczytu temperatury
        /// </summary>
        /// <returns></returns>
        public double GetChamberTemperature()
        {
            return _chamberTemperature.Get();
        }

        /// <summary>
        /// Funcja zwraca obecną wartość odczytu temperatury próbki
        /// </summary>
        /// <returns></returns>
        public double GetSampleTemperature()
        {
            return _sampleTemperature.Get();
        }

        /// <summary>
        /// Funckja zwraca obence nastawy PID
        /// </summary>
        /// <returns></returns>
        public LakeShorePidValue GetCurrentLakeShorePidValue()
        {
            return _currentLakeShorePidValue.Get();
        }

        /// <summary>
        /// Zwraca informację o obecnej jednostce temperatury
        /// </summary>
        /// <returns></returns>
        public LakeShore.TemperatureUnit GetCurrentLakeShoreTemperatureUnit()
        {
            return _lakeShoreTemperatureUnit.Get();
        }

        /// <summary>
        /// Zwraca informację o obecnie wykorzystywanym typie kontroli
        /// </summary>
        /// <returns></returns>
        public LakeShore.ControlType GetCurrentLakeShoreControlType()
        {
            return _lakeShoreControlType.Get();
        }

        // Funkcje Kithley
        // --------------------------------------------------

        /// <summary>
        /// Funkcja służy do dodwania nowego pomiaru przez interfejs pomiarowy
        /// </summary>
        /// <param name="measurement">Wartość pomiaru</param>
        internal void PushNewMeasurement(KithleyMeasurement measurement)
        {
            lock (_lockMeasurements)
            {
                _measurements.Add(measurement);
            }
            NewMeasuermentEvent?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funkcja służy do zmiany informacji o parametrach wykonywania pomiarów
        /// </summary>
        /// <param name="initData">Obecnie nastawione parametry</param>
        internal void SetMeasurementParameters(KithleySweeperInitData param)
        {
            _currentKithleyInitData.Set(param);
            NewMeasurementParametersEvent?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Funkcja służy do uzyskiwania nieprzetworzonych n ostatnich pomiarów
        /// </summary>
        /// <param name="count">Ilość pozyskiwanych pomiarów</param>
        /// <returns>Lista z pomiarami. Jeżeli pomiarów było mniej niż n ma ona tylko liczbę możliwych do uzyskania pomiarów.</returns>
        public List<KithleyMeasurement> TryGetLastMeasurements(int count)
        {
            List<KithleyMeasurement> lastMeasurements = new List<KithleyMeasurement>();
            if (count == 1)
            {
                lock (_lockMeasurements)
                {
                    if (_measurements.Count > 0)
                        lastMeasurements.Add(_measurements.Last());
                }
            }
            if (count > 1)
            {
                lock (_lockMeasurements)
                {
                    for (int i = _measurements.Count - 1; i > _measurements.Count - 1 - count; i--)
                    {
                        if (i < 0)
                            break;
                        lastMeasurements.Add(_measurements[i]);
                    }
                }
            }
            return lastMeasurements;
        }

        /// <summary>
        /// Zwraca ilość wykonanych pomiarów
        /// </summary>
        /// <returns></returns>
        public int GetMeasurementsCount()
        {
            lock (_lockMeasurements)
            {
                return _measurements.Count;
            }
        }

        /// <summary>
        /// Funkcja służy do pozyskiwania wszyskich pomiarów
        /// </summary>
        /// <returns></returns>
        public List<KithleyMeasurement> GetAllMeasurements()
        {
            lock (_lockMeasurements)
            {
                return new List<KithleyMeasurement>(_measurements);
            }
        }

        /// <summary>
        /// Funkcja służy do uzyskiwania informacji o parametrach wykonywania pomiarów
        /// </summary>
        /// <returns></returns>
        public KithleySweeperInitData? GetCurrentParameters()
        {
            return _currentKithleyInitData.Get();
        }

        /// <summary>
        /// Funkcja czyści listę przechowywanych pomiarów
        /// </summary>
        public void ClearMeasurementPool()
        {
            lock (_lockMeasurements)
            {
                _measurements = new List<KithleyMeasurement>();
            }
            NewMeasuermentEvent?.Invoke(new object(), EventArgs.Empty);
        }

        // Funkcje generacji pliku zapisu
        // --------------------------------------------------

        /// <summary>
        /// Funkcja służy do generowania zawartości pliku zapisu dla danych pomiarowych
        /// </summary>
        /// <returns>Zawartość pliku CSV</returns>
        public string GenerateCSV()
        {
            return GenerateSaveContent(',');
        }

        /// <summary>
        /// Funkcja służy do generowania zawartości pliku zapisu dla danych pomiarowych
        /// </summary>
        /// <returns>Zawartość pliku TSV</returns>
        public string GenerateTSV()
        {
            return GenerateSaveContent('\t');
        }

        /// <summary>
        /// Funkcja generująca zawartość pliku zapisu
        /// </summary>
        /// <param name="delimiter">Znak odzielający od siebie wartości</param>
        /// <returns>Zawartość pliku zapisu</returns>
        private string GenerateSaveContent(char delimiter)
        {
            // Tworzenie funkcji i ciągów formatujących
            Func<double, string> doubleFormater =
                number =>
                {
                    // Miejsca znaczące - nie używane ponieważ generowało często np. 12E-8
                    //return number.ToString("G6", CultureInfo.InvariantCulture);
                    // Wszystkie miejsca całkowite i 8 dziesiętnych
                    return number.ToString("0.00000000", CultureInfo.InvariantCulture);
                };
            string dateFormat = "yyyy-MM-dd";
            string timeFormat = "HH:mm:ss.fff";

            // Pobieranie danych
            List<KithleyMeasurement> measurements = GetAllMeasurements();
            if (measurements.Count == 0)
                return "";

            // Tworzenie zmiennej wyjściowej
            StringBuilder result = new StringBuilder();

            // Tworzenie informacji nagłówkowej
            result.Append("Data");
            for (int i = 0; i < measurements.Count; i++)
                result.Append(delimiter + measurements[i].TimeStamp.ToString(dateFormat));
            result.Append(Environment.NewLine);

            result.Append("Czas");
            for (int i = 0; i < measurements.Count; i++)
                result.Append(delimiter + measurements[i].TimeStamp.ToString(timeFormat));
            result.Append(Environment.NewLine);

            result.Append("Temperatura");
            for (int i = 0; i < measurements.Count; i++)
                result.Append(delimiter + doubleFormater(measurements[i].Temperature));
            result.Append(Environment.NewLine);

            // Tworzenie informacji o wartościach pomiaru
            result.AppendLine("Napięcie" + delimiter + "Prąd");
            for (int j = 0; j < measurements[0].Length; j++)
            {
                // Częstotliwość próbki
                result.Append(doubleFormater(measurements[0].Voltage[j]));
                for (int i = 0; i < measurements.Count; i++)
                {
                    result.Append(delimiter + doubleFormater(measurements[i].Current[j]));
                }
                result.Append(Environment.NewLine);
            }
            result.AppendLine("Napięcie" + delimiter + "Rezystancja");
            for (int j = 0; j < measurements[0].Length; j++)
            {
                // Częstotliwość próbki
                result.Append(doubleFormater(measurements[0].Voltage[j]));
                for (int i = 0; i < measurements.Count; i++)
                {
                    result.Append(delimiter + doubleFormater(measurements[i].Resistance[j]));
                }
                result.Append(Environment.NewLine);
            }
            

            result.Remove(result.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            // Zwracanie wyniku
            return result.ToString();
        }

        /// <summary>
        /// Funkcja wczytuje dane pomiarowe z pliku TSV.
        /// Jeżeli wczytanie było udane obecne bufory zostają wyczyszczone i zamienione na wczytane wartości.
        /// </summary>
        /// <param name="fileContent">Zawartość pliku TSV</param>
        /// <returns>Czy udało się wczytać dane</returns>
        public bool LoadFromTSV(string fileContent)
        {
            return LoadFromSaveFile(fileContent, '\t');
        }

        /// <summary>
        /// Funkcja wczytuje dane pomiarowe z pliku CSV.
        /// Jeżeli wczytanie było udane obecne bufory zostają wyczyszczone i zamienione na wczytane wartości.
        /// </summary>
        /// <param name="fileContent">Zawartość pliku CSV</param>
        /// <returns>Czy udało się wczytać dane</returns>
        public bool LoadFromCSV(string fileContent)
        {
            return LoadFromSaveFile(fileContent, ',');
        }

        private bool LoadFromSaveFile(string fileContent, char delimiter)
        {
            // Parametry parsera
            int headerLineCount = 5; // Liczba lini informacyjnych: Czas, Temperatura, Nagłówki
            int valueBlocksCount = 2; // Ilość bloków danych Prąd, Rezystancja
            string dateFormat = "yyyy-MM-dd";
            string timeFormat = "HH:mm:ss.fff";

            fileContent = fileContent.Trim();
            if (string.IsNullOrEmpty(fileContent))
            {
                Log.Error("KithleyStore.LoadFromSaveFile-Invalid empty data string");
                return false;
            }

            // Podział danych
            string[] lines = fileContent.Split('\n');
            if (lines.Length <= headerLineCount)
            {
                Log.Error("KithleyStore.LoadFromSaveFile-To little data for parser");
                return false;
            }
            List<string[]> fields = new List<string[]>(lines.Length);
            for (int i = 0; i < lines.Length; i++)
                fields.Add(lines[i].Trim().Split(delimiter));

            // Tworzenie wczytywanej listy pomiarów
            int measurementCount = fields[0].Length - 1; // Ilość pomiarów
            List<KithleyMeasurement> result = new List<KithleyMeasurement>(measurementCount);

            // Wczytywanie wartości globalnych
            int measurementLength = (fields.Count - headerLineCount) / valueBlocksCount; // Ilość punktów w 1 pomiarze
            double[] voltage = new double[measurementLength];
            for (int j = 0; j < measurementLength; j++)
            {
                try
                {
                    voltage[j] = double.Parse(fields[4 + j][0], CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"KithleyStore.LoadFromSaveFile-Invalid frequency string on position: [{4 + j + 1}, 1]");
                    return false;
                }
            }

            // Wczytywanie wartość per measurement
            for (int i = 0; i < measurementCount; i++)
            {
                DateTime timeStamp;
                try
                {
                    timeStamp = DateTime.ParseExact(
                    fields[0][i + 1] + " " + fields[1][i + 1],
                    dateFormat + " " + timeFormat,
                    CultureInfo.InvariantCulture
                    );
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"KithleyStore.LoadFromSaveFile-Invalid date for measurement: {i + 1}");
                    return false;
                }

                double temperature;
                try
                {
                    temperature = double.Parse(fields[2][i + 1], CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"KithleyStore.LoadFromSaveFile-Invalid temperature string on position: [3, {i + 2}]");
                    return false;
                }

                double[] current = new double[measurementLength];
                double[] resistance = new double[measurementLength];
                for (int j = 0; j < measurementLength; j++)
                {
                    int parsedParameter = 0;
                    try
                    {
                        current[j] = double.Parse(fields[4 + parsedParameter + (measurementLength * parsedParameter) + j][i + 1], CultureInfo.InvariantCulture);
                        parsedParameter++;
                        resistance[j] = double.Parse(fields[4 + parsedParameter + (measurementLength * parsedParameter) + j][i + 1], CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"KithleyStore.LoadFromSaveFile-Invalid parameter string on position: [{4 + parsedParameter + (measurementLength * parsedParameter) + j + 1}, {i + 2}]");
                        return false;
                    }
                }

                result.Add(new KithleyMeasurement
                {
                    Voltage = voltage,
                    Current = current,
                    Resistance = resistance,
                    TimeStamp = timeStamp,
                    Length = measurementLength,
                    Temperature = temperature
                });
            }

            // Zapisywanie wyniku parsowania
            ClearMeasurementPool();
            lock (_lockMeasurements)
            {
                _measurements = result;
            }
            NewMeasuermentEvent?.Invoke(new object(), EventArgs.Empty);
            return true;
        }
    }
}
