using System;
using System.Collections.Generic;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Grupa komend systemowych nie wykorzystywana przy interpretacji.
    /// Jest ona potrzebna jedynie na potrzeby autouzupełniania, korekty itp.
    /// </summary>
    internal sealed class CommandGroupFunc : CommandGroup
    {
        public CommandGroupFunc() : base("func", "Grupa funkcyjna")
        {
            RegisterCommand(
                "repeat",
                PlaceholderFunction,
                (List<object> p1, RunEstimatorContainer p2) => {; },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Int,
                        0,
                        null
                        )
                },
                "Pętla wykonująca się n razy",
                Environment.NewLine + "\t" + Environment.NewLine + "func.end"
                );
            RegisterCommand(
                "end",
                PlaceholderFunction,
                (List<object> p1, RunEstimatorContainer p2) => {; },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>(),
                "Komenda zamykająca pętlę"
                );
            RegisterCommand(
                "wait",
                PlaceholderFunction,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint(p2.GetLastTemperature()); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Int,
                        0,
                        null
                        )
                },
                "Funkcja wstrzymująca wykonanie programu na określoną ilość milisekund"
                );
            RegisterCommand(
                "longWait",
                PlaceholderFunction,
                (List<object> p1, RunEstimatorContainer p2) => { p2.PushNewPoint(p2.GetLastTemperature()); },
                new List<Tuple<ConvertableNumericTypes, object?, object?>>
                {
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Int,
                        0,
                        null),
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Int,
                        0,
                        60),
                    new Tuple<ConvertableNumericTypes, object?, object?>(
                        ConvertableNumericTypes.Int,
                        0,
                        60),
                },
                "Funkcja wstrzymująca wykonanie programu na określony czas HH:mm:ss");
        }

        private static void PlaceholderFunction(List<object> param)
        {
            throw new NotSupportedException("Ta funkcja nie powinna być uruchamiana");
        }
    }
}
