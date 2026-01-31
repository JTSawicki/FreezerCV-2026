using LabServices.DataTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.FlowSensor
{
    public partial class FlowSensorController : HardwareController
    {
        /// <summary>Obecna wartość przepływu wody w l/min</summary>
        private LockedProperty<double> _currentFlow = new LockedProperty<double>(-1);
        /// <summary>
        /// Publiczny event wywoływany przy odczycie nowego przepływu
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewCurrentFlowEvent;

        /// <summary>Obecna wartość przepływu wody w l/min</summary>
        private LockedProperty<double> _targetFlow = new LockedProperty<double>(-1);
        /// <summary>
        /// Publiczny event wywoływany przy odczycie nowego przepływu
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewTargetFlowEvent;


        /// <summary>Czy wifi połączone</summary>
        private LockedProperty<bool> _sensorArmed = new LockedProperty<bool>(false);
        /// <summary>Czy wifi połączone</summary>
        private LockedProperty<bool> _sensorAlarm = new LockedProperty<bool>(false);
        /// <summary>Czy wifi połączone</summary>
        private LockedProperty<bool> _wifiConnected = new LockedProperty<bool>(false);
        /// <summary>
        /// Publiczny event wywoływany przy zmianie stanu czujnika: _sensorArmed, _sensorAlarm, _wifiConnected
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy
        /// </summary>
        public event EventHandler? NewSensorStateEvent;

        // Funkcje Globalne
        // --------------------------------------------------

        /// <summary>
        /// Ustawia endpoint obecnego przepływu
        /// </summary>
        /// <param name="flow">Przepływ w l/min</param>
        private void SetCurrentFlow(double flow)
        {
            _currentFlow.Set(flow);
            NewCurrentFlowEvent?.Invoke(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Zwraca obecny przepływ w l/min
        /// </summary>
        /// <returns></returns>
        public double GetCurrentFlow() =>
            _currentFlow.Get();

        private void SetTargetFlow(double flow)
        {
            _targetFlow.Set(flow);
            NewTargetFlowEvent?.Invoke(new object(), EventArgs.Empty);
        }

        public double GetTargetFlow() =>
            _targetFlow.Get();

        private void SetSensorArmed(bool enabled)
        {
            _sensorArmed.Set(enabled);
            NewSensorStateEvent?.Invoke(new object(), EventArgs.Empty);
        }

        public bool GetSensorArmed() =>
            _sensorArmed.Get();

        private void SetSensorAlarm(bool enabled)
        {
            _sensorAlarm.Set(enabled);
            NewSensorStateEvent?.Invoke(new object(), EventArgs.Empty);
        }

        public bool GetSensorAlarm() =>
            _sensorAlarm.Get();

        private void SetWifiConnected(bool enabled)
        {
            _wifiConnected.Set(enabled);
            NewSensorStateEvent?.Invoke(new object(), EventArgs.Empty);
        }

        public bool GetWifiConnected() =>
            _wifiConnected.Get();
    }
}
