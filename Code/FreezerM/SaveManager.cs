using System;
using System.IO;

namespace FreezerM
{
    public static class SaveManager
    {
        private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly char DirectorySeparator = Path.DirectorySeparatorChar;

        /// <summary>Główny folder aplikacji</summary>
        public static readonly string AppFolder
            = AppDataFolder + DirectorySeparator + "FreezerApp_by_JanSawicki";
        /// <summary>Folder zapisu logów</summary>
        public static readonly string AppFolder_Logs
            = AppFolder + DirectorySeparator + "Logs";
        /// <summary>Folder zapisu ustawień</summary>
        public static readonly string AppFolder_Settings
            = AppFolder + DirectorySeparator + "Settings";
        /// <summary>Domyślny folder zapisu plików użytkownika</summary>
        public static readonly string AppFolder_UserData
            = AppFolder + DirectorySeparator + "UserData";

        /// <summary>Maksymalny czas długości życia pliku logów określony w dniach</summary>
        private static readonly int _maxLogFileLifeInDays = 365; // Około jeden rok
        /// <summary>Prefix nazwy pliku logów. NIE ZMIENIAĆ!!!</summary>
        private const string LogFilePrefix = "PiecykLogs_";
        /// <summary>Formater daty dla nazwy pliku logów. NIE ZMIENIAĆ!!!</summary>
        private const string LogFileDateFormater = "yyyy-MM-dd";

        static SaveManager()
        {
            // Inicjalizacja systemu plików
            try
            {
                if (!Directory.Exists(AppFolder))
                {
                    Directory.CreateDirectory(AppFolder);
                }
                if (!Directory.Exists(AppFolder_Logs))
                {
                    Directory.CreateDirectory(AppFolder_Logs);
                }
                if (!Directory.Exists(AppFolder_Settings))
                {
                    Directory.CreateDirectory(AppFolder_Settings);
                }
                if (!Directory.Exists(AppFolder_UserData))
                {
                    Directory.CreateDirectory(AppFolder_UserData);
                }
            }
            catch (Exception e)
            {
                // Obsługa błędu inicjalizacji
                throw new InvalidOperationException("SaveManager-Błąd inicjalizacji systemu plików :(", e);
            }

            RemoveOldLogFiles();
        }

        /// <summary>
        /// Funkcja zwraca ścieżkę do pliku logów na dzień dzisiejszy
        /// </summary>
        /// <param name="fileExtension">Rozszerzenie pliku. Domyślnie: json</param>
        /// <returns>Pełna ścieżka do pliku</returns>
        public static string GetLogFilePath(string fileExtension = "json")
        {
            string logFilePath =
                AppFolder_Logs + DirectorySeparator +
                LogFilePrefix +
                DateTime.Now.ToString(LogFileDateFormater) +
                "." + fileExtension;
            if (File.Exists(logFilePath))
                return logFilePath;

            StreamWriter file = File.CreateText(logFilePath);
            file.Close();
            return logFilePath;
        }

        /// <summary>
        /// Usuwa zbyt stare pliki logów
        /// </summary>
        private static void RemoveOldLogFiles()
        {
            int todayCoefficient =
                DateTime.Now.Day +
                DateTime.Now.Month * 31 +
                DateTime.Now.Year * 365;
            string[] ListOfLogFiles = Directory.GetFiles(AppFolder_Logs);
            foreach (string LogFile in ListOfLogFiles)
            {
                try
                {
                    int indexOfDate = LogFile.IndexOf(LogFilePrefix) + LogFilePrefix.Length;
                    string dateString = LogFile.Substring(indexOfDate, 10);
                    int logFileCoefficient =
                        int.Parse(dateString.Substring(0, 4)) * 365 +
                        int.Parse(dateString.Substring(5, 2)) * 31 +
                        int.Parse(dateString.Substring(8, 2));
                    if (todayCoefficient - logFileCoefficient > _maxLogFileLifeInDays)
                    {
                        File.Delete(LogFile);
                    }
                }
                catch
                {
                    ; // Brak działań. Najprawdopodobniej nienadająca się do parsowania nazwa pliku
                }
            }
        }

        /// <summary>
        /// Zwraca zawartość pliku ustawień
        /// </summary>
        /// <param name="file">Nazwa pliku np. "MainSettings.json"</param>
        /// <returns>Zawartość pliku</returns>
        public static string ReadSettingsFile(string file)
        {
            string fullPath = AppFolder_Settings + DirectorySeparator + file;
            return File.ReadAllText(fullPath);
        }

        /// <summary>
        /// Funkcja nadpisuje zawartość pliku ustawień.
        /// Jeżeli plik nie istnieje to go tworzy.
        /// </summary>
        /// <param name="file">Nazwa pliku np. "MainSettings.json"</param>
        /// <param name="content">Nowa zawartość pliku</param>
        public static void WriteToSettingsFile(string file, string content)
        {
            string fullPath = AppFolder_Settings + DirectorySeparator + file;
            File.WriteAllText(fullPath, content);
        }

        /// <summary>
        /// Sprawdza czy podany plik ustawień istnieje
        /// </summary>
        /// <param name="file">Nawa pliku</param>
        /// <returns>Czy plik istnieje</returns>
        public static bool SettingsFileExist(string file)
        {
            string fullPath = AppFolder_Settings + DirectorySeparator + file;
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Odczytuje zawartość pliku
        /// </summary>
        /// <param name="path">Pełna ścieżka do pliku</param>
        /// <returns>Zawartość pliku</returns>
        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Funkcja nadpisuje zawartość pliku.
        /// Jeżeli plik nie istnieje to go tworzy.
        /// </summary>
        /// <param name="path">Pełna ścieżka do pliku</param>
        /// <param name="content">Zawartość pliku</param>
        public static void WritetoFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Funkcja zwraca przykładową nazwę pliku nie występującego w folderze zapisów urzytkownika.
        /// Np. dla nameMaster="plik", fileExtension="txt" => "plik12.txt"
        /// </summary>
        /// <param name="nameMaster">Główny człon nazwy pliku</param>
        /// <param name="fileExtension">Rozszerzenie</param>
        /// <returns>Nowa nazwa pliku</returns>
        public static string GetExampleSaveFileName(string nameMaster, string fileExtension, string folderPath)
        {
            string result = $"{nameMaster}.{fileExtension}";
            ulong counter = 0;
            while (File.Exists(folderPath + DirectorySeparator + result))
            {
                counter++;
                result = $"{nameMaster}_{counter}.{fileExtension}";
            }
            return result;
        }
    }
}
