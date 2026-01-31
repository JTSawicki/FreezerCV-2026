using FreezerM.CodeProcesor;
using LabServices.GpibHardware;
using LabServices.FlowSensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using FreezerGUI.CodeCommands;

namespace FreezerGUI
{
    public static class BootStrapper
    {
        // Zmienne

        /// <summary>Ustawienia aplikacji</summary>
        public static AppSettings Settings;

        /// <summary>Dispatcher aplikacji</summary>
        public static Dispatcher Dispatcher;

        /// <summary>Kontroler magistrali Gpib</summary>
        public static GpibHardwareController GpibController;

        /// <summary>Kontroler sensora przepływu</summary>
        public static FlowSensorController FlowSensorController;

        /// <summary>Grupa komend skryptów</summary>
        public static CommandMaster CommandMaster;


        // Funkcje
        static BootStrapper()
        {
            Settings = AppSettings.Load();
            GpibController = new GpibHardwareController();
            FlowSensorController = new FlowSensorController();

            CommandMaster = new CommandMaster();
            CommandMaster.RegisterGroup(new CommandGroupLakeShore());
            CommandMaster.RegisterGroup(new CommandGroupKithley());
        }

        internal static void Start(Dispatcher _dispatcher)
        {
            BootStrapper.Dispatcher = _dispatcher;
        }

        internal static void Stop()
        {
            if (GpibController.IsActive())
            {
                GpibController.StopController();
            }
            if (FlowSensorController.IsActive())
            {
                FlowSensorController.StopController();
            }
        }
    }
}
