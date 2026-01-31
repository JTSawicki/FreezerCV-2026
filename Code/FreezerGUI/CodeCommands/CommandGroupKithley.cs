using System;
using System.Collections.Generic;
using System.Threading;
using FreezerM;
using FreezerM.CodeProcesor;
using LabServices.GpibHardware;
using Serilog;

namespace FreezerGUI.CodeCommands
{
    /// <summary>
    /// Grupa komend programu dla mostka pomiarowego Kithley
    /// </summary>
    internal sealed class CommandGroupKithley : CommandGroup
    {
        public CommandGroupKithley() : base("keithley", "Grupa kontroli miernika Kithley")
        {
            RegisterCommand(
                "sweep",
                Sweep,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewMeasurement(); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>(),
                "Wykonanie pomiaru impedancji(czeka na koniec)"
                );
        }

        /// <summary>
        /// Funkcja wywołuje pomiar i czeka na jego zakończenie
        /// </summary>
        /// <param name="param"></param>
        private static void Sweep(List<object> param)
        {
            if (!BootStrapper.GpibController.IsKithleyConnected())
            {
                Log.Error("CommandGroupKithley-Attempting to invoke a command:sweep with Keithley controller turned off");
                return;
            }
            int measurementCount = BootStrapper.GpibController.GetMeasurementsCount();
            BootStrapper.GpibController.PushCommand(GpibCommands.Sweep);
            while(measurementCount == BootStrapper.GpibController.GetMeasurementsCount())
                Thread.Sleep(50);
        }
    }
}
