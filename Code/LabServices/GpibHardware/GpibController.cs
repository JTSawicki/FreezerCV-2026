using LabServices.Exceptions;
using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LabServices.GpibHardware
{
    /// <summary>
    /// GPIB bus controller class
    /// </summary>
    public sealed class GpibController : IDisposable
    {
        /// <summary>Address of the Gpib C++ controller in memory 🤣</summary>
        private IntPtr _cppController;
        /// <summary>Maximum data buffer size when connected. Initialized in this.Start</summary>
        private int MaxBufferSize;

        /// <summary>Is the controller active</summary>
        public bool IsActive { get; private set; }
        /// <summary>Is a device connected</summary>
        public bool IsConnected { get; private set; }
        /// <summary>Address of the connected device</summary>
        public int? DeviceAddress { get; private set; }

        public GpibController()
        {
            IsActive = false;
            IsConnected = false;
            DeviceAddress = null;
        }

        // Public functions
        // --------------------------------------------------

        /// <summary>
        /// Starts the connection to the bus
        /// Must be closed later -> Dispose
        /// </summary>
        public void Start()
        {
            IsActive = true;
            _cppController = CppCreateController();
            CppSesionStart(_cppController);
            CheckForError();
            MaxBufferSize = CppGetBufferSize(_cppController);
        }

        /// <summary>
        /// Establishes a connection to a device on the bus
        /// </summary>
        /// <param name="address">Device address</param>
        /// <exception cref="NotConnectedException"></exception>
        public void DeviceConnect(int address)
        {
            if (!IsActive)
            {
                throw new NotConnectedException("Gpib controller is not started");
            }

            DeviceAddress = address;
            string addressString = $"GPIB0::{address}::INSTR";
            CppDeviceConnect(_cppController, addressString);
            CheckForError();
            IsConnected = true;
        }

        /// <summary>
        /// Disconnects from a device on the bus
        /// </summary>
        public void DeviceDisconnect()
        {
            CheckIfConnected();
            CppDeviceDisconnect(_cppController);
            CheckForError();
            IsConnected = false;
        }

        /// <summary>
        /// Query to the connected device on the bus
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string Query(string command)
        {
            CheckIfConnected();
            StringBuilder sb = new StringBuilder(MaxBufferSize);
            CppQuery(_cppController, command, sb);
            CheckForError();
            return sb.ToString();
        }

        /// <summary>
        /// Query to the connected device on the bus with large response size
        /// </summary>
        /// <param name="command"></param>
        /// <param name="maxResponseSize">Maximum response size</param>
        /// <returns></returns>
        public string QueryBigResponse(string command, int maxResponseSize)
        {
            CheckIfConnected();
            StringBuilder sb = new StringBuilder(maxResponseSize);
            CppBigResponseQuery(_cppController, command, sb, maxResponseSize);
            try
            {
                CheckForError();
            } catch (Exception ex)
            {
                Log.Error("Error on QueryBigResponse", ex);
                throw new Exception("Error on QueryBigResponse");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Write to the connected device on the bus
        /// </summary>
        /// <param name="command"></param>
        public void Write(string command)
        {
            CheckIfConnected();
            StringBuilder sb = new StringBuilder(MaxBufferSize);
            CppWrite(_cppController, command);
            CheckForError();
        }

        /// <summary>
        /// Read from the connected device on the bus
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            CheckIfConnected();
            StringBuilder sb = new StringBuilder(MaxBufferSize);
            CppRead(_cppController, sb);
            CheckForError();
            return sb.ToString();
        }

        /// <summary>
        /// Enables SRQ event handling
        /// </summary>
        public void EnableSRQ()
        {
            CppEnableSRQ(_cppController);
        }

        /// <summary>
        /// Waits for SRQ event
        /// </summary>
        /// <param name="waitTimeout">Wait timeout</param>
        public void WaitForSRQ(int waitTimeout)
        {
            CppWaitForSRQ(_cppController, waitTimeout);
        }

        // Private functions
        // --------------------------------------------------

        /// <summary>
        /// Checks if the C++ library threw errors and throws them in C# if so
        /// </summary>
        /// <exception cref="CppGpibException"></exception>
        private void CheckForError()
        {
            bool hasError = CppHasError(_cppController);
            if (hasError)
            {
                StringBuilder sb = new StringBuilder(MaxBufferSize);
                CppGetError(_cppController, sb);
                throw new CppGpibException(sb.ToString());
            }
        }

        /// <summary>
        /// Checks if connected to a device
        /// </summary>
        /// <exception cref="NotConnectedException"></exception>
        private void CheckIfConnected()
        {
            if (!IsActive)
            {
                throw new NotConnectedException("Gpib controller is not started");
            }
            if (!IsConnected)
            {
                throw new NotConnectedException("Gpib controller is not connected to any device");
            }
        }

        /// <summary>
        /// Releases connection resources
        /// </summary>
        public void Dispose()
        {
            if (IsActive && IsConnected)
            {
                CppDeviceDisconnect(_cppController);
                CppSesionStop(_cppController);
            }
            if (IsActive)
            {
                CppSesionStop(_cppController);
            }
        }

        // Dll imports
        // --------------------------------------------------

        [DllImport("GpibCppController.dll")]
        private static extern IntPtr CppCreateController();

        [DllImport("GpibCppController.dll")]
        private static extern void CppSesionStart(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern void CppSesionStop(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern void CppDeviceConnect(IntPtr obj, string address);

        [DllImport("GpibCppController.dll")]
        private static extern void CppDeviceDisconnect(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern int CppGetBufferSize(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern bool CppHasError(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern void CppGetError(IntPtr obj, StringBuilder output);

        [DllImport("GpibCppController.dll")]
        private static extern void CppWrite(IntPtr obj, string command);

        [DllImport("GpibCppController.dll")]
        private static extern void CppRead(IntPtr obj, StringBuilder response);

        [DllImport("GpibCppController.dll")]
        private static extern void CppQuery(IntPtr obj, string command, StringBuilder response);

        [DllImport("GpibCppController.dll")]
        private static extern void CppBigResponseQuery(IntPtr obj, string command, StringBuilder response, int responseSize);

        [DllImport("GpibCppController.dll")]
        private static extern void CppEnableSRQ(IntPtr obj);

        [DllImport("GpibCppController.dll")]
        private static extern void CppWaitForSRQ(IntPtr obj, int waitTimeout);
    }
}
