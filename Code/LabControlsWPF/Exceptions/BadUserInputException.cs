using System;

namespace LabControlsWPF.Exceptions
{
    /// <summary>
    /// Błąd niepoprawnych danych wprowadzonych przez użytkownika
    /// </summary>
    [Serializable]
    public class BadUserInputException : Exception
    {
        public BadUserInputException() { }
        public BadUserInputException(string message) : base(message) { }
        public BadUserInputException(string message, Exception innerException) : base(message, innerException) { }
    }
}
