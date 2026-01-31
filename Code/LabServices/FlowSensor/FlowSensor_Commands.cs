using LabServices.DataTemplates;
using LabServices.GpibHardware;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.FlowSensor
{
    public enum FlowSensorCommands : ushort
    {
        /// <summary>
        /// Ta komenda nic nie robi 😛
        /// Domyślnie: brak
        /// Ograniczenie: brak
        /// Parametry: brak
        /// </summary>
        DummyCommand,
        /// <summary>
        /// Uzbraja czujnik
        /// Parametry: brak
        /// </summary>
        Arm,
        /// <summary>
        /// Rozbraja czujnik
        /// Parametry: brak
        /// </summary>
        Disarm,
        /// <summary>
        /// Ustawia cel przepływu
        /// Parametry: double(przepływ l/min)
        /// </summary>
        SetTargetFlow,
        /// <summary>
        /// Ustawia parametry połączenia WiFi
        /// Parametry: string(nazwa sieci), string(hasło)
        /// </summary>
        SetWiFi,
        /// <summary>
        /// Ustawia parametry podłączenia E-Mail
        /// Parametry: string(serwer), int(serwer port), string(login), string(hasło)
        /// </summary>
        SetEMail,
        /// <summary>
        /// Wywołuje ponowne połączenie czujnika z siecią WiFi
        /// Parametry: brak
        /// </summary>
        ReconnectWiFi,
        /// <summary>
        /// Wywołuje wysłanie testowej wiadomości przez czujnik
        /// Parametry: brak
        /// </summary>
        SendTestEmail,
    }

    public partial class FlowSensorController : HardwareController
    {
        protected override void RegisterCommands()
        {
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.DummyCommand, DummyCommand);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.Arm, Arm);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.Disarm, Disarm);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.SetTargetFlow, SetTargetFlow);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.SetWiFi, SetWiFi);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.SetEMail, SetEMail);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.ReconnectWiFi, ReconnectWiFi);
            _commandPool.RegisterCommand((ushort)FlowSensorCommands.SendTestEmail, SendTestEmail);
        }

        private void DummyCommand(List<object> param) { }

        private void Arm(List<object> param)
        {
            _serial.Write("06");
        }

        private void Disarm(List<object> param)
        {
            _serial.Write("07");
        }

        private void SetTargetFlow(List<object> param)
        {
            // Sprawdzenie poprawności danych wejściowych
            if (param.Count != 1 ||
                param[0] is not double)
            {
                Log.Error($"Bad parameter in FlowSensorController.SetTargetFlow");
                return;
            }

            _serial.Write($"05{((double)param[0]).ToString("00.00", CultureInfo.InvariantCulture)}");
        }

        private void SetWiFi(List<object> param)
        {
            // Sprawdzenie poprawności danych wejściowych
            if (param.Count != 2 ||
                param[0] is not string ||
                param[1] is not string)
            {
                Log.Error($"Bad parameters in FlowSensorController.SetWiFi");
                return;
            }

            _serial.Write($"22{(string)param[0]}");
            _serial.Write($"23{(string)param[1]}");
        }

        private void SetEMail(List<object> param)
        {
            // Sprawdzenie poprawności danych wejściowych
            if (param.Count != 4 ||
                param[0] is not string ||
                param[1] is not int ||
                param[2] is not string ||
                param[3] is not string)
            {
                Log.Error($"Bad parameters in FlowSensorController.SetWiFi");
                return;
            }

            _serial.Write($"32{(string)param[0]}");
            _serial.Write($"33{(int)param[1]}");
            _serial.Write($"34{(string)param[2]}");
            _serial.Write($"35{(string)param[3]}");
            _serial.Write($"36{(string)param[3]}");
        }

        private void ReconnectWiFi(List<object> param)
        {
            _serial.Write("24");
        }

        private void SendTestEmail(List<object> param)
        {
            _serial.Write("31");
        }
    }
}
