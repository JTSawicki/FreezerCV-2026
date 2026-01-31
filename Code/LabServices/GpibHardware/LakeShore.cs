namespace LabServices.GpibHardware
{
    public static class LakeShore
    {
        public const ushort MinPIDParameter = 0;
        public const ushort MaxPParameter = 999;
        public const ushort MaxIParameter = 999;
        public const ushort MaxDParameter = 200;

        /// <summary>
        /// Typ kontroli sterownika
        /// </summary>
        public enum ControlType : int
        {
            Manual = 0,
            AutoP = 1,
            AutoPI = 2,
            AutoPID = 3,
        }

        public enum TemperatureUnit
        {
            Celcius,
            Kelvins,
        }
    }
}
