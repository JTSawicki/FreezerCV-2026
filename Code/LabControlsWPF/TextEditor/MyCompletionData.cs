using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace LabControlsWPF.TextEditor
{
    /// <summary>
    /// Klasa podpowiedzi komendy lub grupy komend dla edytora kodu
    /// </summary>
    public class MyCompletionData : ICompletionData
    {
        /// <summary>Ilość parametrów komendy. Dla grupy nieistotne</summary>
        private int _parameterCount;
        /// <summary>Tekst komendy. Dla grupy null</summary>
        private string? _command;
        /// <summary>Tekst grupy komend</summary>
        private string _commandGroup;
        /// <summary>Opis komendy lub grupy</summary>
        private string _description;
        /// <summary>Dodatkowy tekst do wprowadzenia</summary>
        private string? _additionalText;

        /// <summary>
        /// Tworzy obiekt podpowiedzi
        /// </summary>
        /// <param name="command">Nazwa komendy</param>
        /// <param name="commandGroup">Nazwa grupy</param>
        /// <param name="parameterCount">Liczba parametrów komendy</param>
        /// <param name="description">Opis komendy dla null domyślny</param>
        private MyCompletionData(string? command, string commandGroup, int parameterCount, string? description = null, string? additionalText = null)
        {
            _command = command;
            _commandGroup = commandGroup;
            _parameterCount = parameterCount;
            if(description == null)
                description = $"Opis komendy: {command}";
            _description = description;
            _additionalText = additionalText;
        }

        public static MyCompletionData GetCommandCompletionData(string command, string commandGroup, int parameterCount, string? description = null, string? additionalText = null) =>
            new MyCompletionData(command, commandGroup, parameterCount, description, additionalText);

        internal static MyCompletionData GetGroupCompletionData(string commandGroup, string? description = null) =>
            new MyCompletionData(null, commandGroup, 0, description);

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // Pozyskiwanie informacji o miejscu wprowadzania
            DocumentLine line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            string textLine = textArea.Document.GetText(line.Offset, line.Length).Trim();

            // Wykrywanie wprowadzanego elementu
            if (_command == null)
            {
                // Uzupełnianie grupy komend
                int charsToDelete = textLine.Length;
                textArea.Document.Replace(completionSegment, _commandGroup.Substring(charsToDelete, Text.Length - charsToDelete));
            }
            else
            {
                // Uzupełanie komendy
                int charsToDelete = textLine.Length - (textLine.IndexOf('.') + 1);
                textArea.Document.Replace(completionSegment, _command.Substring(charsToDelete, Text.Length - charsToDelete));

                // Wprowadzanie tekstu dodatkowego
                if (_additionalText != null)
                {
                    textArea.Document.Insert(textArea.Caret.Offset, _additionalText);
                    textArea.Caret.Offset -= _additionalText.Length;
                }

                // Wstawianie nawiasów parametrów
                if (_parameterCount > 0)
                {
                    textArea.Document.Insert(textArea.Caret.Offset, "(");
                    for (int i = 0; i < _parameterCount - 1; i++)
                        textArea.Document.Insert(textArea.Caret.Offset, ", ");
                    textArea.Document.Insert(textArea.Caret.Offset, ")");
                    textArea.Caret.Offset -= (_parameterCount - 1) * 2 + 1;
                }
            }
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Inherited parameters space here ↓

        public ImageSource? Image => null;

        public string Text => _command ?? _commandGroup;

        public object Content => Text;

        public object Description => _description;

        public double Priority => 0;
    }
}
