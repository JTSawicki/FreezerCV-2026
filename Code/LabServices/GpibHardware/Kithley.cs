namespace LabServices.GpibHardware
{
    public static class Kithley
    {
        /// <summary>Minimalna wartość zadana dla źródła napięciowego</summary>
        public const double MinSourceVoltage = 0.000005;
        /// <summary>Maksymalna wartość zadana dla źródła napięciowego</summary>
        public const double MaxSourceVoltage = 210;
        /// <summary>Minimalna wartość mierzona dla źródła prądowego</summary>
        public const double MinSenseCurrent = 0.000000001;
        /// <summary>Maksymalna wartość mierzona dla źródła prądowego</summary>
        public const double MaxSenseCurrent = 1.05;
        /// <summary>Maksymalna wartość obciążenia</summary>
        public const double MaxPower = 22;
        /// <summary>Maksymalny czas opóźnienia źródła</summary>
        public const double MaxSourceDelay = 999.9999;
        /// <summary>Maksymalny ilość punktów pomiarowych na sweep programu</summary>
        public const int MaxMeasurementPerSweepCount = 2500;


        /// <summary>
        /// Segmentacja częstotliwości pomiaru
        /// </summary>
        public enum VoltageSegmentation : long
        {
            Linear,
            Logarytmic
        }

        /// <summary>
        /// Metoda podłączenia próbki 2/4 kable
        /// </summary>
        public enum ConnectionType : long
        {
            FourWireTerminal,
            TwoWireTerminal
        }
    }
}
