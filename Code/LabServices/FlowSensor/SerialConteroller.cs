using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SerialPort = System.IO.Ports.SerialPort;

namespace LabServices.FlowSensor
{
    /// <summary>
    /// Kontroler połączenia serial
    /// </summary>
    public class SerialConteroller : IDisposable
    {
        /// <summary>Port COM kontrolera</summary>
        public string Port { get; init; }
        /// <summary>Baudrate połączenia</summary>
        public int Baudrate { get; init; }
        /// <summary>Timeout dla odczytu</summary>
        public int Timeout { get; init; }
        /// <summary>Przerwa po wysłaniu wiadomości dla urządzenia by miało czas przetworzyć</summary>
        public int CommunicationDealy { get; init; }
        private SerialPort _serialPort;

        public SerialConteroller(string port, int baudrate, int timeout, int communicationDealy)
        {
            Port = port;
            Baudrate = baudrate;
            Timeout = timeout;
            CommunicationDealy = communicationDealy;

            _serialPort = new SerialPort(port, baudrate);
            _serialPort.Open();
        }

        /// <summary>
        /// Wysyła komendę
        /// </summary>
        /// <param name="message"></param>
        public async void Write(string message)
        {
            _serialPort.WriteLine(message);
            await Task.Delay(CommunicationDealy);
        }

        /// <summary>
        /// Wykonuje zapytanie do urządzenia
        /// </summary>
        /// <param name="message">Komenda</param>
        /// <returns>Odpowiedź / null dla braku lub timeout</returns>
        public string? Query(string message)
        {
            WaitForClearRead();

            Write(message);

            Stopwatch sw = Stopwatch.StartNew();
            string response = string.Empty;
            while (response.Equals("") && sw.ElapsedMilliseconds < Timeout)
            {
                if (_serialPort.BytesToRead > 0)
                {
                    response = _serialPort.ReadLine().Trim();
                }
            }
            sw.Stop();

            if (response.Equals(string.Empty))
                return null;
            return response;
        }

        public void Dispose()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Funkcja oczekuje na czysty bufor i brak nowych zapisów
        /// </summary>
        private async void WaitForClearRead()
        {
            while (true)
            {
                if (_serialPort.BytesToRead == 0)
                    return;

                while (_serialPort.BytesToRead > 0)
                    _serialPort.ReadByte();

                await Task.Delay(50);
            }
        }
    }
}
