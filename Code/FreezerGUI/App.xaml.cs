using FreezerM;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FreezerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly int _aplicationVersion = 2;
        private readonly int _aplicationSubVersion = 1;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Inicjalizacja Loggera(musi być pierwsza)
            SetupSerilogLogger();

            // Inicjalizacja DependencyInjection
            BootStrapper.Start(this.Dispatcher);

            // Inicjalizacja schematu kolorów
            ApplayTheme();

            Log.Information("Begining of aplication initialization");
            Log.Information($"Aplication version: {_aplicationVersion}.{_aplicationSubVersion}");
            try
            {
                // Tworzenie pierwszego okna
                Windows.ConnectWindow connectWindow = new Windows.ConnectWindow();
                connectWindow.Show();
                // Inicjalizacja bazowa(wymagana)
                base.OnStartup(e);
            }
            catch
            {
                Log.Fatal("Aplication fail to start");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Aplication ending");

            // Wyłączanie DependencyInjection
            BootStrapper.Stop();
            // Wyłączanie Loggera
            Log.CloseAndFlush();
            // Zakończenie bazowe(wymagane)
            base.OnExit(e);
        }

        /// <summary>
        /// Funkcja inicjalizuje logger Serilog dla całego programu
        /// </summary>
        private void SetupSerilogLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .MinimumLevel.Verbose()
                .WriteTo.Debug(outputTemplate: "[Serilog] {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception} {Properties:j}{NewLine}")
                .WriteTo.File(formatter: new Serilog.Formatting.Json.JsonFormatter(), path: SaveManager.GetLogFilePath("json"), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .CreateLogger();
        }

        /// <summary>
        /// Ustawia wczytany z ustawień schemat kolorów
        /// </summary>
        private void ApplayTheme()
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor((Color)ColorConverter.ConvertFromString(BootStrapper.Settings.PrimaryColor));
            theme.SetSecondaryColor((Color)ColorConverter.ConvertFromString(BootStrapper.Settings.SecondaryColor));

            if (BootStrapper.Settings.TextColorEnforcement)
            {
                var PrimaryTextColor = (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.PrimaryTextColor);
                var SecondaryTextColor = (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.SecondaryTextColor);
                theme.PrimaryLight = new ColorPair(theme.PrimaryLight.Color, PrimaryTextColor);
                theme.PrimaryMid = new ColorPair(theme.PrimaryMid.Color, PrimaryTextColor);
                theme.PrimaryDark = new ColorPair(theme.PrimaryDark.Color, PrimaryTextColor);
                theme.SecondaryLight = new ColorPair(theme.SecondaryLight.Color, SecondaryTextColor);
                theme.SecondaryMid = new ColorPair(theme.SecondaryMid.Color, SecondaryTextColor);
                theme.SecondaryDark = new ColorPair(theme.SecondaryDark.Color, SecondaryTextColor);
            }

            if (BootStrapper.Settings.DarkMode)
                theme.SetBaseTheme(Theme.Dark);
            else
                theme.SetBaseTheme(Theme.Light);

            paletteHelper.SetTheme(theme);
        }
    }
}
