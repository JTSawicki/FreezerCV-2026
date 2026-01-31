using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabControlsWPF.TextEditor
{
    /// <summary>
    /// Klasa zbioru podpowiedzi dla edytora kodu
    /// </summary>
    public class HintPool
    {
        /// <summary>Słownik par (nazwa grupy komend, klasy podpowiedzi do komendy)</summary>
        private Dictionary<string, List<MyCompletionData>> _commandHints;
        /// <summary>Słownik par (nazwa grupy komend, opis grupy)</summary>
        private Dictionary<string, string> _groupsDescriptions;
        /// <summary>Podpowiedzi dla grup komend</summary>
        private Dictionary<string, MyCompletionData> _groupCompletionData;

        public HintPool()
        {
            _commandHints = new Dictionary<string, List<MyCompletionData>>();
            _groupsDescriptions = new Dictionary<string, string>();
            _groupCompletionData = new Dictionary<string, MyCompletionData>();
        }

        /// <summary>
        /// Rejestruje grupę komend
        /// </summary>
        /// <param name="groupName">Nazwa grupy</param>
        /// <param name="commandHints">Podpowiedzi dla grupy</param>
        /// <param name="groupDescription">Opis grupy dla null domyślny</param>
        public void RegisterGroup(string groupName, List<MyCompletionData> commandHints, string? groupDescription = null)
        {
            _commandHints.Add(groupName, new List<MyCompletionData>(commandHints));
            if(groupDescription == null)
                groupDescription = $"Opis grupy komend: {groupName}";
            _groupsDescriptions.Add(groupName, groupDescription);
            _groupCompletionData.Add(groupName, MyCompletionData.GetGroupCompletionData(groupName, groupDescription));
        }

        /// <summary>Zwraca listę zarejestrowanych grup</summary>
        public List<string> GroupsList =>
            _commandHints.Keys.ToList();

        /// <summary>Zwraca opis grupy</summary>
        public string GetGroupDescription(string groupName) =>
            _groupsDescriptions[groupName];

        /// <summary>Zwraca podpowiedź komendy grupy</summary>
        public MyCompletionData GetGroupCompletionData(string groupName) =>
            _groupCompletionData[groupName];

        /// <summary>Zwraca listę podpowiedzi komend należących do grupy</summary>
        public IEnumerable<MyCompletionData> GetCommands(string groupName) =>
            _commandHints[groupName];
    }
}
