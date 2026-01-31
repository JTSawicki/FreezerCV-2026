using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Kontener danych na potrzeby estymacji przebiegu progrmau
    /// </summary>
    public class RunEstimatorContainer
    {
        private List<Tuple<double, double>> _temperatures;
        private List<Tuple<double, double>> _measurements;

        /// <summary>Wygenerowana lista punktów temperatury wykresu estymacji przebiegu programu</summary>
        public List<Tuple<double, double>> Temperatures => new List<Tuple<double, double>>(_temperatures);

        /// <summary>Wygenerowana lista punktów temperatury wykresu estymacji przebiegu programu</summary>
        public List<Tuple<double, double>> Measurements => new List<Tuple<double, double>>(_measurements);

        public RunEstimatorContainer()
        {
            _temperatures = new List<Tuple<double, double>>
            {
                new Tuple<double, double>(1, 1)
            };
            _measurements = new List<Tuple<double, double>>();
        }

        /// <summary>
        /// Zwraca temperaturę ostatniego punktu
        /// </summary>
        /// <returns></returns>
        public double GetLastTemperature()
        {
            return _temperatures.Last().Item2;
        }

        /// <summary>
        /// Ustawia nowy punkt o podanej temperaturze
        /// </summary>
        /// <param name="temperature"></param>
        public void PushNewPoint(double temperature, bool addStep = true)
        {
            double x = _temperatures.Last().Item1;
            if (addStep)
                x += 1;
            _temperatures.Add(
                new Tuple<double, double>(x, temperature)
                );
        }

        /// <summary>
        /// Ustawia nowy punkt pomiaru
        /// </summary>
        public void PushNewMeasurement()
        {
            _measurements.Add(_temperatures.Last());
        }
    }
}
