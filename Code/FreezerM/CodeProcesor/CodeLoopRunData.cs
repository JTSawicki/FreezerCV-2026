using System.Collections.Generic;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Obiekt danych inicjujący pętlę wywołania kodu
    /// </summary>
    public class CodeLoopRunData
    {
        public List<CodeCommandContainer> Code { get; init; }
        public CommandMaster Master { get; init; }

        public CodeLoopRunData(List<CodeCommandContainer> code, CommandMaster master)
        {
            Code = code;
            Master = master;
        }
    }
}
