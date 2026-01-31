using LabServices.DataTemplates;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    /// <summary>
    /// Klasa zapewniająca interfejs komunikacyjny z urządzeniami na magistrali Gpib
    /// </summary>
    public partial class GpibHardwareController : HardwareController
    {
        /// <summary>
        /// Funkcja uruchamia wątek kontrolera jeżeli nie jest już aktywny
        /// </summary>
        public void StartController(GpibHardwareInitData param) =>
            InternalStartTask(param);

        /// <summary>
        /// Wysyła komendę do wykonania przez kontroler
        /// </summary>
        /// <param name="command">ID komendy</param>
        /// <param name="param">Lista parametrów komendy</param>
        public void PushCommand(GpibCommands command, List<object>? param = null) =>
            InternalPushCommand((ushort)command, param);
    }
}
