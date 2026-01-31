using LabServices.DataTemplates;
using LabServices.GpibHardware;
using System.Collections.Generic;

namespace LabServices.FlowSensor
{
    /// <summary>
    /// Klasa zapewniająca interfejs komunikacyjny z czujnikiem przepływu
    /// </summary>
    public partial class FlowSensorController : HardwareController
    {
        /// <summary>
        /// Funkcja uruchamia wątek kontrolera jeżeli nie jest już aktywny
        /// </summary>
        /// <param name="com">Port com na którym znajduje się czujnik</param>
        public void StartController(string com) =>
            InternalStartTask(com);

        /// <summary>
        /// Wysyła komendę do wykonania przez kontroler
        /// </summary>
        /// <param name="command">ID komendy</param>
        /// <param name="param">Lista parametrów komendy</param>
        public void PushCommand(FlowSensorCommands command, List<object>? param = null) =>
            InternalPushCommand((ushort)command, param);
    }
}
