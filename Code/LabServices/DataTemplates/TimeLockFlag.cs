using System.Diagnostics;

namespace LabServices.DataTemplates
{
    /// <summary>
    /// Klasa parametru blokady czasowej
    /// Pozwala nastawić blokadę i sprawdzić czy jej czas już miną
    /// </summary>
    internal class TimeLockFlag
    {
        /// <summary>Licznik czasu</summary>
        private Stopwatch watch;
        /// <summary>Czas blokady w ms</summary>
        private long lockPeriod;

        public TimeLockFlag()
        {
            watch = Stopwatch.StartNew();
            // Zatrzymywanie licznika ponieważ blokada wynosi zero i nie ma potrzeby odliczania
            watch.Stop();
            lockPeriod = 0;
        }

        /// <summary>
        /// Funkcja ustawia czas blokady i uruchamia pomiar czasu
        /// </summary>
        /// <param name="milisecondsTime">Czas blokady w ms</param>
        public void SetLock(long milisecondsTime)
        {
            watch.Restart();
            lockPeriod = milisecondsTime;
        }

        /// <summary>
        /// Czy blokada jes aktywna
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            if(watch.ElapsedMilliseconds >= lockPeriod)
            {
                if(watch.IsRunning)
                    watch.Stop();
                return false;
            }
            return true;
        }
    }
}
