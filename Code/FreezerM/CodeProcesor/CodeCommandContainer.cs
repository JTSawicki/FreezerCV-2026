using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Klasa do przechowywania przetworzonych komend.
    /// </summary>
    public class CodeCommandContainer
    {
        /// <summary>Numer lini z edytora dla komendy</summary>
        public int LineNumber;
        /// <summary>Grupa komend</summary>
        public string CommandGroup;
        /// <summary>Komenda</summary>
        public string Command;
        /// <summary>Lista parametrów</summary>
        public List<object> Parameters;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineNumber">Numer lini z edytora</param>
        /// <param name="commandGroup">Grupa komend</param>
        /// <param name="command">Komenda</param>
        /// <param name="parameters">Lista parametrów</param>
        public CodeCommandContainer(int lineNumber, string commandGroup, string command, List<object> parameters)
        {
            LineNumber = lineNumber;
            CommandGroup = commandGroup;
            Command = command;
            Parameters = new List<object>(parameters);
        }
    }
}
