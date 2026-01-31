using System;

namespace LabServices.Exceptions
{
    /// <summary>
    /// Błąd braku połączenia z urządzeniem na lini GPIB
    /// </summary>
    [Serializable]
    public sealed class NotConnectedException : Exception
    {
        public NotConnectedException() { }
        public NotConnectedException(string message) : base(message) { }
        public NotConnectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
