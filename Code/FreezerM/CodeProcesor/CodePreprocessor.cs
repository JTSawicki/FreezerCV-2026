using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreezerM.Exceptions;
using Serilog;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Klasa zawiera funkcje wstępnie przetwarzające kod programu
    /// </summary>
    public static class CodePreprocessor
    {
        /// <summary>
        /// Konwertuje wstępnie kod programu wprowadzony przez użytkownika do postaci nadającej się do przesłania do wykonania
        /// </summary>
        /// <param name="code">Kod programu wprowadzonego przez użytkownika</param>
        /// <returns>Lista obiektów komend</returns>
        /// <exception cref="CodePreprocessorErrorException">Błąd konwersji kodu. Zawiera informacje do wyświetlenia użytkownikowi.</exception>
        public static List<CodeCommandContainer> ProcessCode(string code, CommandMaster master)
        {
            List<CodeCommandContainer> result;
            try
            {
                List<string> TmpCodeList = SplitCode(code);
                Tuple<List<int>, List<string>> TmpCommandListWithLineNumers = ClearBlank(TmpCodeList);
                List<string[]> CommandsAndParameters = SplitToCommandAndParameters(TmpCommandListWithLineNumers.Item2);
                result = CommandMake(CommandsAndParameters, TmpCommandListWithLineNumers.Item1, master);
                CheckProgramIntegrity(result);
            }
            catch(CodePreprocessorErrorException)
            {
                Log.Information("CodePreprocessor.ProcessCode-Bad user input code");
                throw;
            }
            catch(Exception ex)
            {
                Log.Error("CodePreprocessor.ProcessCode-Unidentified, unhandled user input code error", ex);
                throw;
            }
            return result;
        }

        public static RunEstimatorContainer GenerateEstimation(List<CodeCommandContainer> code, CommandMaster master)
        {
            RunEstimatorContainer estimator = new RunEstimatorContainer();
            EstimateFrom(code, 0, 1, master, estimator);
            return estimator;

        }


        private static int _currentlyEstimatedCommandPosition;
        private static void EstimateFrom(List<CodeCommandContainer> code, int line, int times, CommandMaster master, RunEstimatorContainer estimator)
        {
            for (int i = 0; i < times; i++)
            {
                _currentlyEstimatedCommandPosition = line;
                while (true)
                {
                    // Pobieranie nowej komendy jeżeli jest to możliwe
                    if (_currentlyEstimatedCommandPosition >= code.Count)
                        return;
                    CodeCommandContainer command = code[_currentlyEstimatedCommandPosition];

                    // Wykonywanie komend
                    if (command.CommandGroup.Equals("func"))
                    {
                        if (command.Command.Equals("end"))
                            break;
                        if (command.Command.Equals("repeat"))
                            EstimateFrom(
                                    code,
                                    _currentlyEstimatedCommandPosition + 1,
                                    (int)command.Parameters[0],
                                    master,
                                    estimator
                                    );
                        else
                            master.GetCommandGroup(command.CommandGroup)
                                .ExecuteEstimation(command.Command, command.Parameters, estimator);
                    }
                    else
                    {
                        master.GetCommandGroup(command.CommandGroup)
                            .ExecuteEstimation(command.Command, command.Parameters, estimator);
                    }
                    // Zwiększanie licznika estymowanej lini
                    _currentlyEstimatedCommandPosition += 1;
                }
            }
        }

        /// <summary>
        /// Funkcja dzieli wejściowy kod na wiersze i usuwa komentarze.
        /// </summary>
        /// <param name="inputCode_">Wejściowy kod</param>
        /// <returns>Kod podzielony na listę komend.</returns>
        private static List<string> SplitCode(string inputCode_)
        {
            // Podmienianie znaku końca lini na '\n'
            // Może to mieć znaczenie w wypadku gdy występuje wieloznakowy koniec lini
            inputCode_ = inputCode_.Replace(Environment.NewLine, "\n");

            // Dzielenie obiektu string
            List<string> SplitedInput = new List<string>(inputCode_.Split('\n'));

            // Interacja po wszystkich wierszach
            for (int i = 0; i < SplitedInput.Count; i++)
            {
                int startIndex = SplitedInput[i].IndexOf('#');
                // Sprawdzenie czy występuje komentarz
                if (startIndex != -1)
                {
                    SplitedInput[i] = SplitedInput[i].Remove(startIndex, SplitedInput[i].Length - startIndex);
                }
            }

            // Zwracanie wartości
            return SplitedInput;
        }

        /// <summary>
        /// Funkcja usuwa białe znaki oraz usuwa puste linie.
        /// </summary>
        /// <param name="inputCode_">Wejściowa lista lini kodu z usuniętymi komentarzami</param>
        /// <returns>(Numer lini kodu, Lista wyczyszczonych komend)</returns>
        private static Tuple<List<int>, List<string>> ClearBlank(List<string> inputCode_)
        {
            // Tworzenie kopi listy wejściowej(w celu uniknięcia modyfikowania listy wejściowej)
            List<string> InputCodeCopy = new List<string>(inputCode_);
            // Tworzenie listy numerów
            List<int> LineNumerList = new List<int>();

            // Usuwanie białych znaków
            for (int i = 0; i < InputCodeCopy.Count; i++)
            {
                InputCodeCopy[i] = InputCodeCopy[i].Replace(" ", "");
                InputCodeCopy[i] = InputCodeCopy[i].Replace("\t", "");

                // Dodawanie numeru lini do listy jeżeli linia różna od "" (jeżeli taka jest to zostanie zaraz usunięta)
                if (!InputCodeCopy[i].Equals(""))
                {
                    LineNumerList.Add(i);
                }
            }

            // Usuwanie pustych lini
            InputCodeCopy.RemoveAll(s => s.Equals(""));

            return new Tuple<List<int>, List<string>>(LineNumerList, InputCodeCopy);
        }

        /// <summary>
        /// Funkcja dzieli komendy na tablicę o zawartości {"komenda", "parametr 1", "parametr 2", "parametr 3", ... }
        /// Kod powinien byś podzielony z usuniętymi komentarzami i białymi znakami.
        /// </summary>
        /// <param name="codeLines">Wejściowa lista lini kodu.</param>
        /// <returns>(grupakomend, komenda, parametry ...)</returns>
        private static List<string[]> SplitToCommandAndParameters(List<string> codeLines)
        {
            // Inicjalizowanie listy wyjściowej
            List<string[]> OutputList = new List<string[]>(codeLines.Count);

            // Interacja po wszystkich komendach
            for (int i = 0; i < codeLines.Count; i++)
            {
                string programCommand = codeLines[i];
                // Usuwanie znaku ')' używanego przy wywołaniu funkcji i niepotrzebnego przy dalszej obróbce danych
                programCommand = programCommand.Replace(")", string.Empty);
                // Wydzielanie grupy komend(ze względu na brak możliwości podziału na '.' przy double)
                string[] partialySplitedCommand = 
                    programCommand.Split(new[] { '.' }, 2)
                    .Where(val => !val.Equals(string.Empty))
                    .ToArray();
                // Podział komendy na poszczególne elementy i usuwanie pustych lini
                List<string> tmpList = new List<string>();
                foreach (string elem in partialySplitedCommand)
                {
                    tmpList.AddRange(
                        elem.Split(new[] { '(', ',' })
                        .Where(val => !val.Equals(string.Empty)
                        ));
                }
                // Zapisywanie do listy wyjściowej
                OutputList.Add(tmpList.ToArray());
            }
            return OutputList;
        }


        /// <summary>
        /// Funkcja konwertuje podzielone wcześniej linie kodu na komendy.
        /// Podczas konwersji jest wykonywane sprawdzenie poprawności komend oraz ich danych w tym ograniczeń wartościowych.
        /// Liczność zbiorów danych wejściowych musi być równa.
        /// </summary>
        /// <param name="splitedCommands">Podzielone komendy</param>
        /// <param name="lineNumberCommands">Numer lini komend</param>
        /// <returns>Lista poprawnych obiektów komend</returns>
        /// <exception cref="ArgumentException">Błąd wywołania funkcji gdy wejścia są różnoliczne</exception>
        /// <exception cref="CodePreprocessorErrorException">Błąd konwersji kodu. Zawiera informacje do wyświetlenia użytkownikowi.</exception>
        private static List<CodeCommandContainer> CommandMake(List<string[]> splitedCommands, List<int> lineNumberCommands, CommandMaster master)
        {
            // Sprawdzenie równoliczności zbiorów
            if (splitedCommands.Count != lineNumberCommands.Count)
                throw new ArgumentException($"CodePreprocessor.CommandMake-Zbiory danych wejściowych o różnej liczności: {splitedCommands.Count}, {lineNumberCommands.Count}");

            // Tworzenie obiektu wynikowego błędu
            StringBuilder ProcessingErrors = new StringBuilder();

            // Tworzenie obiektu wynikowego
            List<CodeCommandContainer> result = new List<CodeCommandContainer>(splitedCommands.Count);

            // Iteracja po wszystkich komendach
            for(int i = 0; i < splitedCommands.Count; i++)
            {
                string[] command = splitedCommands[i];
                int commandLineNumber = lineNumberCommands[i];
                // Kontrola poprawności typu komendy
                if(command.Length < 2)
                {
                    ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawna komenda -> Brak grupy lub komendy.");
                    goto BreakCommandLoop;
                }
                if (!master.ContainsGroup(command[0]))
                {
                    ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawna komenda -> Grupa komend nie istnieje.");
                    goto BreakCommandLoop;
                }
                if (!master.GetCommandGroup(command[0]).ContainsCommand(command[1]))
                {
                    ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawna komenda -> Komenda nie istnieje.");
                    goto BreakCommandLoop;
                }

                // Parsowanie parametrów
                List<Tuple<ConvertableNumericTypes, object?, object?>>  commandsInfo = master.GetCommandGroup(command[0]).GetParametersInfo(command[1]);
                List<object> commandParameters = new List<object>(commandsInfo.Count);
                if(commandsInfo.Count != command.Length-2)
                {
                    ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawna liczba parametrów: {command.Length - 2}, oczekiwano: {commandsInfo.Count}");
                    goto BreakCommandLoop;
                }
                for(int j = 0; j< commandsInfo.Count; j++)
                {
                    // Konwersja na wartość numeryczną
                    object? param = NumericConverters.StringToNumber(command[j + 2], commandsInfo[j].Item1);
                    if(param == null)
                    {
                        ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawny parametr: {j} -> Niemożliwy do konwersji literał.");
                        goto BreakCommandLoop;
                    }
                    // Sprawdzanie wartości
                    bool hasCorrectValue = NumericConverters.CheckNumberLimits(param, commandsInfo[j].Item2, commandsInfo[j].Item3, commandsInfo[j].Item1);
                    if(!hasCorrectValue)
                    {
                        ProcessingErrors.AppendLine($"W lini: {commandLineNumber + 1} | Nie poprawny parametr: {j} -> Zła wartość.");
                        goto BreakCommandLoop;
                    }
                    // Parametr poprawny
                    commandParameters.Add(param);
                }
                // Komenda poprawna
                result.Add(new CodeCommandContainer(commandLineNumber, command[0], command[1], commandParameters));
            BreakCommandLoop:;
            }

            // Kontrola błędów wykonania
            if(ProcessingErrors.Length != 0)
            {
                throw new CodePreprocessorErrorException(ProcessingErrors.ToString());
            }
            return result;
        }

        /// <summary>
        /// Funkcja odpowiada za sprawdzenie poprawności logiki programu.
        /// Np. czy pętle są zamknięte odpowiednią ilość razy
        /// </summary>
        /// <param name="code"></param>
        /// <exception cref="CodePreprocessorErrorException">Błąd kodu. Zawiera informacje do wyświetlenia użytkownikowi.</exception>
        private static void CheckProgramIntegrity(List<CodeCommandContainer> code)
        {
            // Sprawdzanie poprawności zamknięcia pętli repeat
            int endCounter = 0;
            foreach(CodeCommandContainer command in code)
            {
                if (command.CommandGroup.Equals("func") &&
                    command.Command.Equals("repeat"))
                    endCounter++;
                if (command.CommandGroup.Equals("func") &&
                    command.Command.Equals("end"))
                    endCounter--;
                if(endCounter < 0)
                    throw new CodePreprocessorErrorException($"W lini: {command.LineNumber} | Niepoprawne zakończenie pętli");
            }
            if(endCounter > 0)
                throw new CodePreprocessorErrorException($"Nie zamknięta pętla w programie");
        }
    }
}
