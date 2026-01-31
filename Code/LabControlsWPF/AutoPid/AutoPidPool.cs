using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabControlsWPF.Exceptions;

namespace LabControlsWPF.AutoPid
{
    /// <summary>
    /// Klasa kontenera auto pid dla ikonki menu wyboru
    /// </summary>
    public class AutoPidPool
    {
        /// <summary>Zmienna na potrzeby oznaczenia wersji podczas serializacji do pliku</summary>
        public int SaveVersion { get; set; } = 1;
        /// <summary>
        /// Lista wartości auto pid w kolejności (temperatura[double], P[ushort], I[ushort], D[ushort])
        /// </summary>
        public List<Tuple<string, string, string, string>> StrPidList { get; set; }
        /// <summary>
        /// Domyślna wartość pid w kolejności (P[ushort], I[ushort], D[ushort])
        /// </summary>
        public Tuple<string, string, string> StrDefaultPidValue { get; set; }
        /// <summary>
        /// Ilość wybranych punktów auto pid[int]
        /// </summary>
        public string SelectedAutoPidPointCount { get; set; }

        public AutoPidPool(string defaultPValue, string defaultIValue, string defaultDValue, string selectedAutoPidPointCount)
        {
            StrDefaultPidValue = new Tuple<string, string, string>(defaultPValue, defaultIValue, defaultDValue);
            SelectedAutoPidPointCount = selectedAutoPidPointCount;
            StrPidList = new List<Tuple<string, string, string, string>>();
            for (int i = 0; i < int.Parse(selectedAutoPidPointCount); i++)
                StrPidList.Add(new Tuple<string, string, string, string>("", "", "", ""));
        }

        public AutoPidPool()
        {
            StrDefaultPidValue = new Tuple<string, string, string>("", "", "");
            SelectedAutoPidPointCount = "1";
            StrPidList = new List<Tuple<string, string, string, string>>();
            StrPidList.Add(new Tuple<string, string, string, string>("", "", "", ""));
        }

        /// <summary>
        /// Funkcja parsująca string -> double
        /// </summary>
        /// <param name="value">Wartość parsowana</param>
        /// <returns>Wartość double lub null jeżeli błędny ciąg wejściowy</returns>
        private static double? TryParseDouble(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            value = value.Replace(',', '.');
            try
            {
                return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Funkcja parsująca string -> int
        /// </summary>
        /// <param name="value">Wartość parsowana</param>
        /// <returns>Wartość int lub null jeżeli błędny ciąg wejściowy</returns>
        private static int? TryParseInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            try
            {
                return int.Parse(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Funkcja parsująca string -> ushort
        /// </summary>
        /// <param name="value">Wartość parsowana</param>
        /// <returns>Wartość ushort lub null jeżeli błędny ciąg wejściowy</returns>
        private static ushort? TryParseUshort(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            try
            {
                return ushort.Parse(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Funkcja oblicza wartość auto pid pool
        /// </summary>
        /// <returns>Wartość auto pid pool</returns>
        /// <exception cref="BadUserInputException">Informacja o błędzie wyliczenia. WYŚWIETLIĆ UŻYTKOWNIKOWI W POWIADOMIENIU!!!</exception>
        public SortedList<double, Tuple<ushort, ushort, ushort>> GetPidPool()
        {
            SortedList<double, Tuple<ushort, ushort, ushort>> tmpPidList = 
                new SortedList<double, Tuple<ushort, ushort, ushort>>(new TemperatureComparer());
            for(int i = 0; i < StrPidList.Count; i++)
            {
                double? temperatureValue = TryParseDouble(StrPidList[i].Item1);
                ushort? pValue = TryParseUshort(StrPidList[i].Item2);
                ushort? iValue = TryParseUshort(StrPidList[i].Item3);
                ushort? dValue = TryParseUshort(StrPidList[i].Item4);
                if (temperatureValue == null ||
                    pValue == null ||
                    iValue == null ||
                    dValue == null)
                    throw new BadUserInputException($"Wprowadzono pusty lub wadliwy parametr w auto pid w lini: {i+1}.");
                tmpPidList.Add(
                    temperatureValue.Value,
                    new Tuple<ushort, ushort, ushort>(
                        pValue.Value,
                        iValue.Value,
                        dValue.Value
                        ));
            }
            return tmpPidList;
        }

        /// <summary>
        /// Funkcja oblicza wartość domyślną auto pid
        /// </summary>
        /// <returns>Domyślna wartość auto pid</returns>
        /// <exception cref="BadUserInputException">Informacja o błędzie wyliczenia. WYŚWIETLIĆ UŻYTKOWNIKOWI W POWIADOMIENIU!!!</exception>
        public Tuple<ushort, ushort, ushort> GetDefaultPid()
        {
            ushort? pValue = TryParseUshort(StrDefaultPidValue.Item1);
            ushort? iValue = TryParseUshort(StrDefaultPidValue.Item2);
            ushort? dValue = TryParseUshort(StrDefaultPidValue.Item3);
            if (pValue == null ||
                iValue == null ||
                dValue == null)
                throw new BadUserInputException("Wprowadzono pusty lub wadliwy parametr w domyślny auto pid.");
            return new Tuple<ushort, ushort, ushort>(
                pValue.Value,
                iValue.Value,
                dValue.Value
                );
        }

        /// <summary>
        /// Funkcja oblicza wartość SelectedAutoPidPointCount
        /// </summary>
        /// <returns>SelectedAutoPidPointCount</returns>
        public int GetSelectedAutoPidPointCount()
        {
            return TryParseInt(this.SelectedAutoPidPointCount)!.Value;
        }

        public override bool Equals(object? obj)
        {
            // Porównanie ogólne
            if (obj == null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;

            // Porównanie zawartości
            AutoPidPool tmp = (AutoPidPool)obj;
            if(tmp.StrPidList.Count != this.StrPidList.Count)
                return false;
            for (int i = 0; i < this.StrPidList.Count; i++)
                if (!tmp.StrPidList[i].Equals(this.StrPidList[i]))
                    return false;
            if (!tmp.StrDefaultPidValue.Equals(this.StrDefaultPidValue))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.StrPidList,
                this.StrDefaultPidValue
                );
        }

        /// <summary>
        /// Wewnętrzna klasa implementująca porównanie wartości temperatury
        /// Sortuje w porządku rosnącym
        /// </summary>
        private class TemperatureComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                if (x < y) return -1;
                if (x > y) return 1;
                return 0;
            }
        }
    }
}
