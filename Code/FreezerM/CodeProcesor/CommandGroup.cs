using System;
using System.Collections.Generic;
using System.Linq;

namespace FreezerM.CodeProcesor
{
    /// <summary>
    /// Klasa reprezentująca grupę komend programu.
    /// Może zawierać procedury które powinny być wywoływane jedynie w innym wątku.
    /// </summary>
    public abstract class CommandGroup
    {
        /// <summary>
        /// Nazwa grupy komend
        /// </summary>
        public string GroupName { get; private set; }
        /// <summary>
        /// Opis grupy komend
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Maper nazwy komendy na ID komendy.
        /// Wykorzystywany w celu zmniejszenia zużycia pamięci na przechowywanie kluczy w pozostałych słownikach.
        /// Klucz: Nazwa komendy
        /// Zawartość: ID komendy
        /// </summary>
        private Dictionary<string, int> CommandNameMaper = new Dictionary<string, int>();
        /// <summary>
        /// Prywatny słownik zawierający wiązania do funkcji komend.
        /// Klucz: Nazwa komendy
        /// Zawartość: Funkcja komendy
        /// </summary>
        private Dictionary<int, Action<List<object>>> Commands
            = new Dictionary<int, Action<List<object>>>();
        /// <summary>
        /// Prywatny słownik zawierający wiązania do funkcji estymacji przebiegu programu.
        /// Klucz: Nazwa komendy
        /// Zawartość: Funkcja estymacji
        /// </summary>
        private Dictionary<int, Action<List<object>, RunEstimatorContainer>> EstimationFunctions
            = new Dictionary<int, Action<List<object>, RunEstimatorContainer>>();
        /// <summary>
        /// Puliczny słownik dostępowy do informacji o komendzie.
        /// Klucz: Nazwa komandy
        /// Zawartość: Lista krotek informujących o typie danej wejściowej. Ilość krotek określa ilość danych wejściowych dla komendy.
        /// Zawartość krotki:
        ///  - Typ danej wejściowej
        ///  - Dolne ogranicznie - null jeżeli brak
        ///  - Górne ograniczenie - null jeżeli brak
        ///  - Jednoliniowy opis komendy
        /// </summary>
        private Dictionary<int, List<Tuple<ConvertableNumericTypes, object?, object?>>> CommandParametersInfo
            = new Dictionary<int, List<Tuple<ConvertableNumericTypes, object?, object?>>>();
        /// <summary>
        /// Słownik zawierający krótką informację o komendzie do wyświetlenia w GUI.
        /// Klucz: Nazwa komendy
        /// Zawartość: Informacja o komendzie
        /// </summary>
        private Dictionary<int, string> CommandInfo
            = new Dictionary<int, string>();
        /// <summary>
        /// Dodatkowy text do wstawienia przy wstawianiu komendy z podpowiedzi
        /// </summary>
        private Dictionary<int, string> CommandAdditionalTextToInsert
            = new Dictionary<int, string>();

        public CommandGroup(string groupName, string description)
        {
            GroupName = groupName;
            Description = description;
        }

        /// <summary>
        /// Funkcja odpowiada za akcję zarejestrowania komendy
        /// </summary>
        /// <param name="commandName">Nazwa komendy</param>
        /// <param name="commandFunction">Funkcja komendy</param>
        /// <param name="parameterInformation">
        /// Informacja o parametrach komendy.
        /// Lista krotek informujących o typie danej wejściowej.
        /// Ilość krotek określa ilość danych wejściowych dla komendy.
        /// Zawartość krotki:
        ///  - Typ danej wejściowej
        ///  - Dolne ogranicznie - null jeżeli brak
        ///  - Górne ograniczenie - null jeżeli brak
        ///  - Jednoliniowy opis komendy
        /// </param>
        /// <param name="commandShortDescription">Jednoliniowy krótki opis komendy do wyświetlania w podpowiedziach GUI</param>
        /// <param name="additionalTextToInsert">Text do dodania w GUI po wstawieniu komendy</param>
        protected void RegisterCommand(string commandName, Action<List<object>> commandFunction, Action<List<object>, RunEstimatorContainer> estimationFunction, List<Tuple<ConvertableNumericTypes, object?, object?>> parameterInformation, string commandShortDescription, string additionalTextToInsert = "")
        {
            // Wyznaczanie ID komendy
            int commandID = CommandNameMaper.Count;
            CommandNameMaper[commandName] = commandID;
            // Zapisywanie wartości
            Commands[commandID] = commandFunction;
            EstimationFunctions[commandID] = estimationFunction;
            CommandParametersInfo[commandID] = new List<Tuple<ConvertableNumericTypes, object?, object?>>(parameterInformation);
            CommandInfo[commandID] = commandShortDescription;
            CommandAdditionalTextToInsert[commandID] = additionalTextToInsert;
        }

        /// <summary>
        /// Zwraca informację o komendzie
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <returns>
        /// Lista krotek informacyjnych informacyjna:
        ///  - Typ danej
        ///  - Dolne ograniczenie - null jeżeli brak
        ///  - Górne ograniczenie - null jeżeli brak
        /// </returns>
        public List<Tuple<ConvertableNumericTypes, object?, object?>> GetParametersInfo(string command) =>
            CommandParametersInfo[CommandNameMaper[command]];

        /// <summary>
        /// Zwraca krótki opis komendy dla GUI
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <returns></returns>
        public string GetShortCommandDescription(string command) =>
            CommandInfo[CommandNameMaper[command]];

        /// <summary>
        /// Zwraca text do dodania w GUI po wstawieniu komendy
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <returns></returns>
        public string GetAdditionalTextToInsert(string command) =>
            CommandAdditionalTextToInsert[CommandNameMaper[command]];

        /// <summary>
        /// Funkcja uruchamia komendę jeżeli została zarejestrowana
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <param name="parameterList">Parametry komendy</param>
        /// <returns>True jeżeli komenda istnieje, false w przeciwnym wypadku</returns>
        public bool ExecuteCommand(string command, List<object> parameterList)
        {
            if (!ContainsCommand(command))
                return false;
            Commands[CommandNameMaper[command]].Invoke(parameterList);
            return true;
        }

        /// <summary>
        /// Funkcja uruchamia funkcję estymacji jeżeli komenda została zarejestrowana
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <param name="parameterList">Parametry komendy</param>
        /// <param name="estimatorParameter">Obiekt estymatora</param>
        /// <returns>True jeżeli komenda istnieje, false w przeciwnym wypadku</returns>
        public bool ExecuteEstimation(string command, List<object> parameterList, RunEstimatorContainer estimatorParameter)
        {
            if (!ContainsCommand(command))
                return false;
            EstimationFunctions[CommandNameMaper[command]].Invoke(parameterList, estimatorParameter);
            return true;
        }

        /// <summary>
        /// Zwraca listę nazw zarejestrowanych komend
        /// </summary>
        /// <returns></returns>
        public List<string> GetCommandNames() =>
            CommandNameMaper.Keys.ToList();

        /// <summary>
        /// Zwraca informację czy grupa zawiera podaną komendę.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool ContainsCommand(string command) =>
            CommandNameMaper.ContainsKey(command);
    }
}
