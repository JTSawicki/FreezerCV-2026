using System;
using System.Collections.Generic;
using System.Threading;

namespace FreezerM.CodeProcesor
{
    public static class CodeInterpreter
    {
        /// <summary>Czy wątek jest aktywny</summary>
        private static LockedProperty<bool> _isActive;

        /// <summary>
        /// Która linia kodu jest obecnie interpretowana.
        /// Do ustawiania powinien być używany seter SetCurrentlyInterpretedLine.
        /// </summary>
        private static LockedProperty<int> _currentlyInterpretedLine;
        /// <summary>Która pozycja na liście komend jest obecnie wykonywana</summary>
        private static int _currentlyInterpretedCommandPosition = 0;
        /// <summary>
        /// Publiczny event wywoływany przy zmianie wykonywanej lini kodu.
        /// Event może być wywoływany przez inne wątki - zabezpieczenie po stronie odbiorcy.
        /// </summary>
        public static event EventHandler? NewCurrentlyInterpretedLineEvent;

        static CodeInterpreter()
        {
            _isActive = new LockedProperty<bool>(false);
            _currentlyInterpretedLine = new LockedProperty<int>(-1);
        }

        /// <summary>
        /// Funkcja wywołuje zadany kod, z zadanej pozycji zadaną ilość razy
        /// </summary>
        /// <param name="code">Wykonywany kod programu</param>
        /// <param name="line">Wykonywana linia</param>
        /// <param name="times">Ilość razy wykonania</param>
        private static void InterpretFrom(List<CodeCommandContainer> code, int line, int times, CommandMaster master)
        {
            for(int i = 0; i < times; i++)
            {
                _currentlyInterpretedCommandPosition = line;
                while(true)
                {
                    // Pobieranie nowej komendy jeżeli jest to możliwe
                    if (_currentlyInterpretedCommandPosition >= code.Count)
                        return;
                    CodeCommandContainer command = code[_currentlyInterpretedCommandPosition];
                    SetCurrentlyInterpretedLine(command.LineNumber);

                    // Wykonywanie komend
                    if(command.CommandGroup.Equals("func"))
                    {
                        if (command.Command.Equals("end"))
                            break;
                        if (command.Command.Equals("repeat"))
                            InterpretFrom(
                                    code,
                                    _currentlyInterpretedCommandPosition + 1,
                                    (int)command.Parameters[0],
                                    master
                                    );
                        if (command.Command.Equals("wait"))
                            Thread.Sleep((int)command.Parameters[0]);
                        if(command.Command.Equals("longWait"))
                        {
                            // Odczekiwanie godzin
                            for(int hours=0; hours < (int)command.Parameters[0]; hours++)
                                Thread.Sleep(3600000);
                            // Odczekiwanie minut
                            if((int)command.Parameters[1] > 0)
                                Thread.Sleep((int)command.Parameters[1] * 60 * 1000);
                            // Odczekiwanie sekund
                            if ((int)command.Parameters[2] > 0)
                                Thread.Sleep((int)command.Parameters[2] * 1000);
                        }
                    }
                    else
                    {
                        master.GetCommandGroup(command.CommandGroup)
                            .ExecuteCommand(command.Command, command.Parameters);
                    }
                    // Zwiększanie licznika wywoływanej lini
                    _currentlyInterpretedCommandPosition += 1;
                }
            }
        }

        /// <summary>
        /// Główna funkcja interpretera kodu
        /// </summary>
        /// <param name="param">Obiekt kodu programu do interpretacji -> List(CodeCommandContainer)</param>
        private static void ThreadMain(object? param)
        {
            _currentlyInterpretedCommandPosition = 0;
            _isActive.Set(true);
            CodeLoopRunData runData = (CodeLoopRunData)param!;
            InterpretFrom(runData.Code, 0, 1, runData.Master);
            SetCurrentlyInterpretedLine(-1);
            _isActive.Set(false);
        }

        /// <summary>
        /// Funkcja uruchamia wątek interpretera jeżeli ten nie jest jeszcze aktywny
        /// </summary>
        /// <param name="code">Interpretowany kod programu</param>
        /// <param name="priority">Priorytet uruchomienia wątku</param>
        /// <returns>Czy uruchomiono wątek interpretera</returns>
        public static bool Start(CodeLoopRunData code, ThreadPriority priority = ThreadPriority.Normal)
        {
            if (!_isActive.Get())
            {
                Thread controllerThread = new Thread(ThreadMain);
                controllerThread.Priority = priority;
                controllerThread.Start(code);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Zwraca informację czy wątek interpretera został włączony
        /// </summary>
        /// <returns></returns>
        public static bool InternalIsActive() =>
            _isActive.Get();

        /// <summary>
        /// Zwraca informację o obecnie wykonywanej lini kodu
        /// </summary>
        /// <returns>Dodatni numer lini z indeksacją od 0 lub -1 jeżeli brak działań</returns>
        public static int GetCurrentlyInterpretedLine() =>
            _currentlyInterpretedLine.Get();

        /// <summary>
        /// Ustawia informację o obecnie wykonywanej lini i wywołuje event ustawienia.
        /// </summary>
        /// <param name="line">Dodatni numer lini z indeksacją od 0 lub -1 jeżeli brak działań</param>
        public static void SetCurrentlyInterpretedLine(int line)
        {
            _currentlyInterpretedLine.Set(line);
            NewCurrentlyInterpretedLineEvent?.Invoke(new object(), EventArgs.Empty);
        }
    }
}
