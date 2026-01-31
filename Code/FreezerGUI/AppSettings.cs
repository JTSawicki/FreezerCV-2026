using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FreezerM;
using System.Reflection;
using LabControlsWPF;

namespace FreezerGUI
{
    /// <summary>
    /// Klasa managera ustawień pliku
    /// </summary>
    public class AppSettings
    {
        /// <summary>Nazwa pliku ustawień</summary>
        private const string _filename = "AppSettings.json";

        /// <summary>
        /// Ustawienie króre nic nie robi.
        /// Pozwala wymusić utworzenie pliku ustawień.
        /// </summary>
        public int DummySetting { get; set; }

        /// <summary>Domyślny adres Kithley 2400 na magistrali GPIB</summary>
        public string DefaultKithleyAdress { get; set; }
        /// <summary>Domyślny adres LakeShore 330 na magistrali GPIB</summary>
        public string DefaultLakeShoreAdress { get; set; }
        /// <summary>Domyślny folder zapisu plików danych</summary>
        public string DefaultSaveFolder { get; set; }
        /// <summary>Czy zapisywać ostatni wybrany folder jako domyślny folder zapisu</summary>
        public bool SaveLastAsDefaultSaveFolder { get; set; }

        /// <summary>Główny kolor aplikacji</summary>
        public string PrimaryColor { get; set; }
        /// <summary>Dodatkowy kolor aplikacji</summary>
        public string SecondaryColor { get; set; }
        /// <summary>Kolor tekstu głównego (jeżeli wymuszony)</summary>
        public string PrimaryTextColor { get; set; }
        /// <summary>Kolor tekstu akcentu (jeżeli wymuszony)</summary>
        public string SecondaryTextColor { get; set; }
        /// <summary>Czy wymuszony kolor tekstu</summary>
        public bool TextColorEnforcement { get; set; }
        /// <summary>Czy ciemy tryb</summary>
        public bool DarkMode { get; set; }

        /// <summary>Globalne ustawienie dla czcionek w programie</summary>
        public double GlobalFontSize { get; set; }

        /// <summary>
        /// Inicjalizuje klasę domyślnymi ustawieniami
        /// </summary>
        /// <param name="onAppLoad">Czy wywołanie nastąpiło przy starcie aplikacji</param>
        /// <returns></returns>
        public static AppSettings InitializeDefault(bool onAppLoad = false)
        {
            if (onAppLoad)
            {
            Log.Warning("AppSettings - Odzyskiwanie domyślnych ustawień aplikacji");
            MaterialMessageBox.NewFastMessage(MaterialMessageFastType.InternalError, "Brak/uszkodzony plik ustawień\nOdzyskiwanie domyślnych ustawień aplikacji");
            }

            AppSettings newSettigns = new AppSettings()
            {
                DummySetting = 30,
                DefaultKithleyAdress = "24",
                DefaultLakeShoreAdress = "12",
                DefaultSaveFolder = SaveManager.AppFolder_Settings,
                SaveLastAsDefaultSaveFolder = false,
                PrimaryColor = "#FF00BCD4",
                SecondaryColor = "#FF76FF03",
                PrimaryTextColor = "#FF000000",
                SecondaryTextColor = "#FF000000",
                TextColorEnforcement = false,
                DarkMode = false,
                GlobalFontSize = 14
            };
            newSettigns.Save();
            return newSettigns;
        }

        /// <summary>
        /// Wczytuje ustawienia z pliku
        /// </summary>
        public static AppSettings Load()
        {
            if (!SaveManager.SettingsFileExist(_filename))
            {
                Log.Warning("AppSettings - Plik ustawień nie istnieje");
                return InitializeDefault(true);
            }
            else
            {
                string content = SaveManager.ReadSettingsFile(_filename);
                try
                {
                    if (!IsJsonContainAllParameters(content))
                        throw new Exception("Brak niektórych parametrów w pliku JSON");

                    AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(content);
                    if (settings == null)
                        throw new NullReferenceException("Błąd odczytu pliku");

                    return settings;
                }
                catch (Exception e)
                {
                    Log.Error("AppSettings - Niepowodzenie odczytania ustawień z pliku", e);
                    return InitializeDefault(true);
                }
            }
        }

        /// <summary>
        /// Zapisuje ustawienia do pliku
        /// </summary>
        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            SaveManager.WriteToSettingsFile(_filename, jsonString);
        }

        private static bool IsJsonContainAllParameters(string jsonContent)
        {
            foreach(PropertyInfo property in typeof(AppSettings).GetProperties())
            {
                if(!jsonContent.Contains(property.Name))
                    return false;
            }
            return true;
        }
    }
}
