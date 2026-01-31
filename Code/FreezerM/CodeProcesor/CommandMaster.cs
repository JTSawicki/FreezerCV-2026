using System.Collections.Generic;

namespace FreezerM.CodeProcesor
{
    public class CommandMaster
    {
        /// <summary>Słownik grup komend</summary>
        private Dictionary<string, CommandGroup> _commandGroups = new Dictionary<string, CommandGroup>();

        public CommandMaster()
        {
            RegisterGroup(new CommandGroupFunc());
        }

        /// <summary>
        /// Funkcja rejestrująca grupę komend
        /// </summary>
        /// <param name="group"></param>
        public void RegisterGroup(CommandGroup group)
        {
            _commandGroups.Add(
                group.GroupName,
                group
                );
        }

        public void ExecuteCommand(string group, string command, List<object> param) =>
            _commandGroups[group].ExecuteCommand(command, param);

        public void ExecuteEstimation(string group, string command, List<object> param, RunEstimatorContainer estimator) =>
            _commandGroups[group].ExecuteEstimation(command, param, estimator);

        public CommandGroup GetCommandGroup(string group) =>
            _commandGroups[group];

        public IEnumerable<string> GetGroupNames() =>
            _commandGroups.Keys;

        public bool ContainsGroup(string group) =>
            _commandGroups.ContainsKey(group);
    }
}
