using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    /// <summary>
    /// Obiekt reprezentujący pomiar
    /// </summary>
    public readonly struct KithleyMeasurement
    {
        /// <summary>Napięcie pomiaru</summary>
        public double[] Voltage { get; init; }
        /// <summary>Natężenie prądu</summary>
        public double[] Current { get; init; }
        /// <summary>Opór mierzonego elementu</summary>
        public double[] Resistance { get; init; }
        /// <summary>Czas zakończenia pomiaru</summary>
        public DateTime TimeStamp { get; init; }
        /// <summary>Ilość próbek</summary>
        public int Length { get; init; }
        /// <summary>Temperatura w której wykonano pomiar</summary>
        public double Temperature { get; init; }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            // Porównanie zawartości
            KithleyMeasurement tmp = (KithleyMeasurement)obj;
            if (!Enumerable.SequenceEqual(tmp.Voltage, Voltage))
                return false;
            if (!Enumerable.SequenceEqual(tmp.Current, Current))
                return false;
            if (!Enumerable.SequenceEqual(tmp.Resistance, Resistance))
                return false;
            if (!tmp.TimeStamp.Equals(TimeStamp))
                return false;
            if (tmp.Length != Length)
                return false;
            if (tmp.Temperature != Temperature)
                return false;

            // Wartości równe
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Voltage,
                Current,
                Resistance,
                TimeStamp,
                Length,
                Temperature
                );
        }

        public override string ToString()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
