using System.Collections.Generic;

namespace LabServices.DataTemplates
{
    /// <summary>
    /// Struktura danych komendy dla ControllerCommandPool
    /// </summary>
    public readonly struct ControllerCommandData
    {
        /// <summary>Numer wywoływanej komendy</summary>
        public ushort CommandNumber { get; init; }
        /// <summary>Lista parametrów</summary>
        public List<object> ParamList { get; init; }

        public ControllerCommandData(ushort commandNumber, List<object> paramList)
        {
            CommandNumber = commandNumber;
            ParamList = paramList;
        }
    }
}
