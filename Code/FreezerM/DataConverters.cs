using Serilog;
using System;
using System.Globalization;

namespace FreezerM
{
    public static class NumericConverters
    {
        /// <summary>
        /// Funkcja konwertująca string to zadanego formatu liczbowego
        /// </summary>
        /// <param name="value">Konwertowany string</param>
        /// <param name="type">Zadany typ danych</param>
        /// <returns>Liczba lub null jeżeli nie można przekonwertować z dowolnego powodu</returns>
        public static object? StringToNumber(string value, ConvertableNumericTypes type)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // Standaryzacja wejścia
            value = value.Replace(',', '.');
            value = value.Replace('e', 'E');
            value = value.Replace(" ", string.Empty);
            value = value.Replace("+", string.Empty);

            try
            {
                switch (type)
                {
                    case ConvertableNumericTypes.Short:
                        return short.Parse(value);
                    case ConvertableNumericTypes.UShort:
                        return ushort.Parse(value);
                    case ConvertableNumericTypes.Int:
                        return int.Parse(value);
                    case ConvertableNumericTypes.UInt:
                        return uint.Parse(value);
                    case ConvertableNumericTypes.Long:
                        return long.Parse(value);
                    case ConvertableNumericTypes.ULong:
                        return ulong.Parse(value);
                    case ConvertableNumericTypes.Float:
                        if (value.Contains('E')) // Zapis naukowy np. "1.45E-5"
                        {
                            string[] splited = value.Split('E');
                            return float.Parse(splited[0], NumberStyles.Float, CultureInfo.InvariantCulture) *
                                (float)Math.Pow(10, double.Parse(splited[1]));
                        }
                        else
                            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    case ConvertableNumericTypes.Double:
                        if (value.Contains('E')) // Zapis naukowy np. "1.45E-5"
                        {
                            string[] splited = value.Split('E');
                            return double.Parse(splited[0], NumberStyles.Float, CultureInfo.InvariantCulture) *
                                (double)Math.Pow(10, double.Parse(splited[1]));
                        }
                        else
                            return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Funkcja sprawdza czy zgadzają się limity danych
        /// </summary>
        /// <param name="value">Sprawdzana wartość</param>
        /// <param name="minValue">Minimalna wartość(jeżeli null nie sprawdza)</param>
        /// <param name="maxValue">Maksymalna wartość(jeżeli null nie sprawdza)</param>
        /// <param name="type">Typ danych(dotyczy wszystkich parametrów)</param>
        /// <returns>Czy limity się zgadzają. False również jeżeli podano obiekty niepoprawnego typu na wejście.</returns>
        public static bool CheckNumberLimits(object value, object? minValue, object? maxValue, ConvertableNumericTypes type)
        {
            switch (type)
            {
                case ConvertableNumericTypes.Short:
                    return InternalCheckNumberLimits<short>(value, minValue, maxValue);
                case ConvertableNumericTypes.UShort:
                    return InternalCheckNumberLimits<ushort>(value, minValue, maxValue);
                case ConvertableNumericTypes.Int:
                    return InternalCheckNumberLimits<int>(value, minValue, maxValue);
                case ConvertableNumericTypes.UInt:
                    return InternalCheckNumberLimits<uint>(value, minValue, maxValue);
                case ConvertableNumericTypes.Long:
                    return InternalCheckNumberLimits<long>(value, minValue, maxValue);
                case ConvertableNumericTypes.ULong:
                    return InternalCheckNumberLimits<ulong>(value, minValue, maxValue);
                case ConvertableNumericTypes.Float:
                    return InternalCheckNumberLimits<float>(value, minValue, maxValue);
                case ConvertableNumericTypes.Double:
                    return InternalCheckNumberLimits<double>(value, minValue, maxValue);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Funkcja sprawdza limity wartości przekazanej jako typ generyczny
        /// </summary>
        /// <typeparam name="T">Typ danych(dotyczy wszystkich parametrów)</typeparam>
        /// <param name="value">Sprawdzana wartość</param>
        /// <param name="minValue">Minimalna wartość(jeżeli null nie sprawdza)</param>
        /// <param name="maxValue">Maksymalna wartość(jeżeli null nie sprawdza)</param>
        /// <returns>Czy limity się zgadzają. False również jeżeli podano obiekty niepoprawnego typu na wejście.</returns>
        private static bool InternalCheckNumberLimits<T>(object value, object? minValue, object? maxValue) where T : IComparable
        {
            try
            {
                bool minCondition = true;
                if (minValue != null)
                    minCondition = ((T)value).CompareTo((T)minValue) >= 0;
                bool maxCondition = true;
                if (maxValue != null)
                    maxCondition = ((T)value).CompareTo((T)maxValue) <= 0;
                return minCondition && maxCondition;
            }
            catch
            {
                Log.Error($"NumericConverters.InternalCheckNumberLimits-Invalid input data type: {typeof(T)}, {value.GetType()}, {minValue?.GetType()}, {maxValue?.GetType()}");
                return false;
            }
        }
    }

    /// <summary>
    /// Enum identyfikujący konwertowalne typy liczbowe
    /// </summary>
    public enum ConvertableNumericTypes
    {
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double
    }
}
