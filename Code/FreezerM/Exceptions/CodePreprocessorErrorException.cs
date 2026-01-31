using System;

namespace FreezerM.Exceptions
{
    /// <summary>
    /// Błąd preprocesora kodu programu.
    /// W wiadomości jest zawarta informacja do wyświetlenia użytkownikowi.
    /// </summary>
    public class CodePreprocessorErrorException : Exception
    {
        public CodePreprocessorErrorException() { }
        public CodePreprocessorErrorException(string message) : base(message) { }
        public CodePreprocessorErrorException(string message, Exception innerException) : base(message, innerException) { }
    }
}
