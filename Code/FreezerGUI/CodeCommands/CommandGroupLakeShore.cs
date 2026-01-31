using System;
using System.Threading;
using System.Collections.Generic;
using FreezerM.CodeProcesor;
using FreezerM;
using Serilog;
using LabServices.GpibHardware;

namespace FreezerGUI.CodeCommands
{
    /// <summary>
    /// Grupa komend programu dla sterownika LakeShore
    /// </summary>
    internal sealed class CommandGroupLakeShore : CommandGroup
    {
        public CommandGroupLakeShore() : base("lake", "Grupa kontroli sterownika LakeShore")
        {
            RegisterCommand(
                "changeTemperature",
                ChangeTargetTemperature,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint(p2.GetLastTemperature() + (double)p1[0], false); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Double,
                        null,
                        null
                    )
                },
                "Zmienia nastawę temperatury o zadaną wartość"
                );
            RegisterCommand(
                "setPid",
                SetPid,
                (List<object> p1, RunEstimatorContainer p2) => {; },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?> (
                        ConvertableNumericTypes.UShort,
                        (ushort)1,
                        LakeShore.MaxPParameter 
                    ),
                    new Tuple<ConvertableNumericTypes, object?, object?> (
                        ConvertableNumericTypes.UShort,
                        LakeShore.MinPIDParameter,
                        LakeShore.MaxIParameter
                    ),
                    new Tuple<ConvertableNumericTypes, object?, object?> (
                        ConvertableNumericTypes.UShort,
                        LakeShore.MinPIDParameter,
                        LakeShore.MaxDParameter
                    )
                },
                "Ustawia parametry pid"
                );
            RegisterCommand(
                "setTemperature",
                SetTargetTemperature,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint((double)p1[0], false); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Double,
                        null,
                        null
                        )
                },
                "Ustawiająca nastawę temperatury"
                );
            RegisterCommand(
                "waitUntilTemperature",
                WaitUntilTemperature,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint(p2.GetLastTemperature()); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Double,
                        null,
                        null
                        ),
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Double,
                        null,
                        null
                        )
                },
                "Jeżeli jest to możliwe czeka na osiągnięcie wskazanej temperatury"
                );



            RegisterCommand(
                "waitUntilTargetTemperature",
                WaitUntilTargetTemperature,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint(p2.GetLastTemperature()); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Double,
                        null,
                        null
                        )
                },
                "Czeka na osiągnięcie nastawionej"
                );
        }

        /// <summary>
        /// Zmienia nastawę temperatury o zadaną wartość
        /// </summary>
        /// <param name="param"></param>
        private static void ChangeTargetTemperature(List<object> param)
        {
            if (!BootStrapper.GpibController.IsLakeShoreConnected())
            {
                Log.Error("CommandGroupLakeShore-Attempting to invoke a command:ChangeTargetTemperature with LakeShore controller turned off");
                return;
            }
            if (param.Count != 1)
            {
                Log.Error($"CommandGroupLakeShore.SetTemperature-Invalid parameter count: {param.Count}");
                return;
            }
            if (param[0] is not double)
            {
                Log.Error($"CommandGroupLakeShore.SetTemperature-Invalid parameter type: {param[0].GetType()}");
                return;
            }
            BootStrapper.GpibController.PushCommand(
                GpibCommands.ChangeTargetTemperature,
                new List<object> { param[0] }
                );
        }

        /// <summary>
        /// Ustawia nastawę pid sterownika LakeShore na zadaną wartość
        /// </summary>
        /// <param name="param"></param>
        private static void SetPid(List<object> param)
        {
            if (!BootStrapper.GpibController.IsLakeShoreConnected())
            {
                Log.Error("CommandGroupLakeShore-Attempting to invoke a command:SetPid with LakeShore controller turned off");
                return;
            }
            if (param.Count != 3)
            {
                Log.Error($"CommandGroupLakeShore.SetPid-Invalid parameter count: {param.Count}");
                return;
            }
            if (param[0] is not ushort ||
                param[1] is not ushort ||
                param[2] is not ushort)
            {
                Log.Error($"CommandGroupLakeShore.SetPid-Invalid parameter type: {param[0].GetType()},{param[1].GetType()},{param[2].GetType()}");
                return;
            }
            LakeShorePidValue newPidValue = new LakeShorePidValue(
                (ushort)param[0],
                (ushort)param[1],
                (ushort)param[2]
                );
            BootStrapper.GpibController.PushCommand(
                GpibCommands.SetPid,
                new List<object> { newPidValue }
                );
        }

        /// <summary>
        /// Ustawia docelową temperaturę sterownika LakeShore
        /// </summary>
        /// <param name="param"></param>
        private static void SetTargetTemperature(List<object> param)
        {
            if (!BootStrapper.GpibController.IsLakeShoreConnected())
            {
                Log.Error("CommandGroupLakeShore-Attempting to invoke a command:SetTargetTemperature with LakeShore controller turned off");
                return;
            }
            if (param.Count != 1)
            {
                Log.Error($"CommandGroupLakeShore.SetTemperature-Invalid parameter count: {param.Count}");
                return;
            }
            if (param[0] is not double)
            {
                Log.Error($"CommandGroupLakeShore.SetTemperature-Invalid parameter type: {param[0].GetType()}");
                return;
            }
            BootStrapper.GpibController.PushCommand(
                GpibCommands.SetTargetTemperature,
                new List<object> { param[0] }
                );
        }

        /// <summary>
        /// Jeżeli jest to możliwe czeka na osiągnięcie zadanej temperatury
        /// </summary>
        /// <param name="param"></param>
        private static void WaitUntilTemperature(List<object> param)
        {
            if (!BootStrapper.GpibController.IsLakeShoreConnected())
            {
                Log.Error("CommandGroupLakeShore-Attempting to invoke a command:WaitUntilTemperature with LakeShore controller turned off");
                return;
            }
            if (param.Count != 2)
            {
                Log.Error($"CommandGroupLakeShore.WaitUntilTemperature-Invalid parameter count: {param.Count}");
                return;
            }
            if (param[0] is not double ||
                param[1] is not double )
            {
                Log.Error($"CommandGroupLakeShore.WaitUntilTemperature-Invalid parameter type: {param[0].GetType()},{param[1].GetType()}");
                return;
            }

            // Odczekanie okresu aby zostały zastosowane potencjalne zmiany temperatury.
            Thread.Sleep(100);

            double waitTargetTemperature = (double)param[0];
            double waitTargetTemperatureSpreadMargin = (double)param[1];
            while (true)
            {
                double targetTemperature = BootStrapper.GpibController.GetTargetTemperature();
                double currentTemperature = BootStrapper.GpibController.GetChamberTemperature();
                // Osiągnięcie dokładnego warunku zakończenia
                if (currentTemperature >= waitTargetTemperature - waitTargetTemperatureSpreadMargin &&
                    currentTemperature <= waitTargetTemperature + waitTargetTemperatureSpreadMargin )
                    break;
                // Sprawdzenie osiągalności temperatury
                if ( // Obecny stan leży pomiędzy nastawą oraz oczekiwaną wartością
                     // Wywołanie następuje również jeżli przekroczono temperaturę oczekiwaną i system zmieża do większej zadanej
                     (waitTargetTemperature - currentTemperature) * (currentTemperature - targetTemperature) > 0 ||
                     // Obecny stan leży po przeciwnej stronie nastawy niż oczekiwana wartość
                     Math.Abs(currentTemperature - waitTargetTemperature) > Math.Abs(currentTemperature - targetTemperature)
                   )
                {
                    // Nie można osiągnąć oczekiwanego stanu
                    break;
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Jeżeli jest to możliwe czeka na osiągnięcie zadanej na urządzeniu temperatury
        /// </summary>
        /// <param name="param"></param>
        private static void WaitUntilTargetTemperature(List<object> param)
        {
            if (!BootStrapper.GpibController.IsLakeShoreConnected())
            {
                Log.Error("CommandGroupLakeShore-Attempting to invoke a command:WaitUntilTargetTemperature with LakeShore controller turned off");
                return;
            }
            if (param.Count != 1)
            {
                Log.Error($"CommandGroupLakeShore.WaitUntilTargetTemperature-Invalid parameter count: {param.Count}");
                return;
            }
            if (param[0] is not double)
            {
                Log.Error($"CommandGroupLakeShore.WaitUntilTargetTemperature-Invalid parameter type: {param[0].GetType()}");
                return;
            }

            List<object> waitParams = new List<object>
            {
                BootStrapper.GpibController.GetTargetTemperature(),
                param[0]
            };
            WaitUntilTemperature(waitParams);
        }
    }
}
