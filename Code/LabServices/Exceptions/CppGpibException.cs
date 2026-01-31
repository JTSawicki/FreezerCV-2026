using System;

namespace LabServices.Exceptions
{
    /// <summary>
    /// Błąd pochodzący z sterownika Gpib C++
    /// </summary>
    [Serializable]
    public sealed class CppGpibException : Exception
    {
        public CppGpibException() { }
        public CppGpibException(string message) : base(message) { }
        public CppGpibException(string message, Exception innerException) : base(message, innerException) { }
    }
}
