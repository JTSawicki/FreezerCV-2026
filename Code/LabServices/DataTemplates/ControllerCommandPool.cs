using System;
using System.Collections.Generic;

namespace LabServices.DataTemplates
{
    /// <summary>
    /// Kontroler wywoływania komend dla HardwareController
    /// </summary>
    public class ControllerCommandPool
    {
        /// <summary>Słownik komend do wywołania - kluczowany po numerze funkcji</summary>
        private Dictionary<ushort, Action<List<object>>> _commandActionPool;

        public ControllerCommandPool()
        {
            _commandActionPool = new Dictionary<ushort, Action<List<object>>>();
        }

        /// <summary>
        /// Funkcja rejestruje nową komendę
        /// </summary>
        /// <param name="commandNumber">Numer komendy</param>
        /// <param name="action">Wywoływana akcja komendy</param>
        public void RegisterCommand(ushort commandNumber, Action<List<object>> action)
        {
            _commandActionPool.Add(commandNumber, action);
        }

        /// <summary>
        /// Funkcja wywołuje komendę o podanym numerze
        /// </summary>
        /// <param name="commandNumber">Numer komendy</param>
        /// <param name="param">Lista parametrów</param>
        public void ExecuteCommand(ushort commandNumber, List<object> paramList)
        {
            _commandActionPool[commandNumber].Invoke(paramList);
        }
    }
}
