using System.Text.Json;

namespace LabServices.GpibHardware
{
    public readonly struct KithleySweeperInitData
    {
        /// <summary>Typ podłączenia próbki</summary>
        public Kithley.ConnectionType ConnectionType { get; init; }
        /// <summary>Tryb dzielenia napięcia</summary>
        public Kithley.VoltageSegmentation VoltageSegmentation { get; init; }
        /// <summary>Początkowe(dolne) napięcie. Jednostka V</summary>
        public string VoltageStart { get; init; }
        /// <summary>Końcowe(górne) napięcie. Jednostka V</summary>
        public string VoltageStop { get; init; }
        /// <summary>Krok napięcia(dla trybu LIN). Jednostka V</summary>
        public string VoltageStep { get; init; }
        /// <summary>Poziom ochrony prądowej. Jednostka A</summary>
        public string CurrentProtection { get; init; }
        /// <summary>Ilość punktów napięcia(dla trybu LOG)</summary>
        public int VoltagePoints { get; init; }
        /// <summary>Okres na stabilizację napięcia. Jednostka s</summary>
        public double SourceDelay { get; init; }

        public override string ToString()
        {
            return "KithleyInitData:" + JsonSerializer.Serialize(this);
        }

        public KithleySweeperInitData(Kithley.ConnectionType connectionType, Kithley.VoltageSegmentation voltageSegmentation, string voltageStart, string voltageStop, string voltageStep, string currentProtection, int voltagePoints, double sourceDelay)
        {
            ConnectionType = connectionType;
            VoltageSegmentation = voltageSegmentation;
            VoltageStart = voltageStart;
            VoltageStop = voltageStop;
            VoltageStep = voltageStep;
            CurrentProtection = currentProtection;
            VoltagePoints = voltagePoints;
            SourceDelay = sourceDelay;
        }
    }
}
