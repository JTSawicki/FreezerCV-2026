using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    public class LakeShoreAutoPidPool
    {
        /// <summary>
        /// Lista par - dolna granica temperatury, nastawa pid
        /// Sortowane w kolejności malejącej względem klucza
        /// </summary>
        private SortedList<double, LakeShorePidValue> pidList;
        /// <summary>Wartość domyślna poniżej najniższej na liście lub kiedy ta pusta</summary>
        private LakeShorePidValue defaultValue;

        /// <summary>
        /// Konstruktor obiektu
        /// </summary>
        /// <param name="pidDataList">Lista dodawanych wartości pid</param>
        /// <param name="defaultValue">Wartość domyślna dodawana gdy </param>
        public LakeShoreAutoPidPool(List<KeyValuePair<double, LakeShorePidValue>> pidDataList, LakeShorePidValue defaultValue)
        {
            this.pidList = new SortedList<double, LakeShorePidValue>(new TemperatureInvertedComparer());
            foreach (KeyValuePair<double, LakeShorePidValue> elem in pidDataList)
            {
                this.pidList.Add(elem.Key, elem.Value);
            }
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Funkcja pozyskuje zadaną wartość pid
        /// </summary>
        /// <param name="temperature">Temperatura dla której jest pozyskiwana wartość</param>
        /// <returns></returns>
        public LakeShorePidValue GetValue(double temperature)
        {
            if (pidList.Count > 0) // Kontrola czy są wartości w liście
            {
                foreach (KeyValuePair<double, LakeShorePidValue> elem in pidList)
                    if (temperature >= elem.Key)
                        return elem.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Wewnętrzna klasa implementująca porównanie wartości temperatury
        /// Sortuje w porządku rosnącym
        /// </summary>
        private class TemperatureInvertedComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                if (x < y) return 1;
                if (x > y) return -1;
                return 0;
            }
        }
    }
}
