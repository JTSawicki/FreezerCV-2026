using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace LabServices.DataTemplates
{
    /// <summary>
    /// Klasa realizująca podstawowe funkcjonalności kontrolera sprzętu
    /// </summary>
    public abstract class HardwareController
    {
        /// <summary>Flaga sygnalizująca czy silnik jest aktywny</summary>
        private LockedProperty<bool> _isActive;
        /// <summary>Flaga sygnalizująca zamknięcie pętli</summary>
        private LockedProperty<bool> _killSwitch;
        /// <summary>Kontroler komend</summary>
        protected ControllerCommandPool _commandPool { get; private set; }
        /// <summary>Lista komend wejściowych</summary>
        protected ConcurrentQueue<ControllerCommandData> _controllerCommands { get; private set; }

        protected HardwareController()
        {
            _isActive = new LockedProperty<bool>(false);
            _killSwitch = new LockedProperty<bool>(false);
            _commandPool = new ControllerCommandPool();
            _controllerCommands = new ConcurrentQueue<ControllerCommandData>();
        }
        /// <summary>
        /// Funkcja inicjująca listę komend
        /// </summary>
        protected abstract void RegisterCommands();
        /// <summary>
        /// Funkcja inicjująca działanie kontrolera i uruchamiająca się raz na początku
        /// </summary>
        /// <param name="param">Obiekt inicjalizujący wątek</param>
        protected abstract void Init(object param);
        /// <summary>
        /// Jedna interacja nieskończonej pętli wątku
        /// </summary>
        protected abstract void LoopInteration();
        /// <summary>
        /// Funkcja kończąca działanie kontrolera i uruchamiana raz przy zakończeniu
        /// </summary>
        protected abstract void Finish();

        /// <summary>
        /// Pętla "MaterLoop" głównego wątku
        /// </summary>
        /// <param name="param">Obiekt inicjalizujący wątek</param>
        private void ThreadMain(object? param)
        {
            _isActive.Set(true);
            RegisterCommands();
            Init(param!);
            bool loopFlag = true;
            while(loopFlag)
            {
                LoopInteration();
                if(_killSwitch.Get())
                {
                    Finish();
                    loopFlag = false;
                }
            }
            _isActive.Set(false);
            _killSwitch.Set(false);
        }

        /// <summary>
        /// Funkcja uruchamia wątek kontrolera jeżeli ten nie jest jeszcze aktywny
        /// </summary>
        /// <param name="param">Parametry uruchomienia wątku</param>
        /// <param name="priority">Priorytet uruchomienia wątku</param>
        protected void InternalStartTask(object param, ThreadPriority priority = ThreadPriority.Normal)
        {
            if(!_isActive.Get())
            {
                Thread controllerThread = new Thread(ThreadMain);
                controllerThread.Priority = priority;
                controllerThread.Start(param);
            }
        }

        /// <summary>
        /// Funkcja zleca wyłączenie wątku kontrolera jeżeli ten jest aktywny
        /// </summary>
        public void StopController()
        {
            if(_isActive.Get())
                _killSwitch.Set(true);
        }

        /// <summary>
        /// Zwraca informację czy wątek został włączony
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return _isActive.Get();
        }

        /// <summary>
        /// Wysyła komendę do wykonania przez kontroler
        /// </summary>
        /// <param name="command">ID komendy</param>
        /// <param name="param">Lista parametrów komendy</param>
        protected void InternalPushCommand(ushort command, List<object>? param = null)
        {
            if (param == null)
                param = new List<object>();
            Log.Verbose("Register command {command}");
            _controllerCommands.Enqueue(new ControllerCommandData(command, param));
        }
    }
}
