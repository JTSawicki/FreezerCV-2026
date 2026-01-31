using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

using System.ComponentModel;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace LabControlsWPF.TextEditor
{
    /// <summary>
    /// Logika interakcji dla klasy CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, INotifyPropertyChanged
    {
        public CodeEditor()
        {
            // Inicjalizacja okna i wiązań danych
            InitializeComponent();
            // this.DataContext = this;

            // Pozyskiwanie danych o czcionkach
            FontTypes = new List<string>(200);
            using (System.Drawing.Text.InstalledFontCollection col = new System.Drawing.Text.InstalledFontCollection())
            {
                foreach (System.Drawing.FontFamily fa in col.Families)
                {
                    FontTypes.Add(fa.Name);
                }
            }
            SelectedFontType = "Consolas";

            // Inicjalizacja ustawień edytora
            textEditor.Options.WordWrapIndentation = 15;
            textEditor.Options.ShowEndOfLine = BlankCharactersVisibility;
            textEditor.Options.ShowTabs = BlankCharactersVisibility;
            textEditor.WordWrap = TextWraping;
            textEditor.ShowLineNumbers = LineNumbering;
            textEditor.FontFamily = new FontFamily(SelectedFontType);
            textEditor.FontSize = double.Parse(SelectedFontSize);

            // Subskrybcja eventów
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            // Tworzenie obiektu renderującego
            lineHighlightRenderer = new HighlightCodeLineBackgroundRenderer(textEditor);
            textEditor.TextArea.TextView.BackgroundRenderers.Add(lineHighlightRenderer);

            // Ustawianie wywołania inicjalizacji po zakończeniu ładowania
            this.Loaded += UserControlLoaded;
        }

        /// <summary>
        /// Funkcja uruchamiana po załadowaniu parametrów DependencyProperity do kontrolki oraz zakończeniu jej tworzenia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            // Event zmiany kodu
            textEditor.TextChanged += OnTextChanged;

            // Wczytywanie pliku ustawień kolorowania kodu
            if(File.Exists(HighlightingPatternPath))
                using (XmlTextReader reader = new XmlTextReader(HighlightingPatternPath))
                {
                    textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
        }

        /// <summary>Klasa renderu wyświetlania podkreślająca obecnie wykonywaną linię kodu</summary>
        private HighlightCodeLineBackgroundRenderer lineHighlightRenderer;

        /// <summary>Implementacja eventu INotifyPropertyChanged</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Implementacja funkcji wywołania INotifyPropertyChanged</summary>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Control dependency properties space below ↓

        /// <summary>Obecnie wykonywana linia kodu, null gdy brak</summary>
        public int CurrentlyExecutedLineOfCode
        {
            get { return (int)GetValue(CurrentlyExecutedLineOfCodeProperty); }
            set { SetValue(CurrentlyExecutedLineOfCodeProperty, value); }
        }
        public static readonly DependencyProperty CurrentlyExecutedLineOfCodeProperty =
            DependencyProperty.Register("CurrentlyExecutedLineOfCode", typeof(int), typeof(CodeEditor),
                new UIPropertyMetadata(-1, CurrentlyExecutedLineOfCodePropertyCallback));

        /// <summary>Ścieżka do pliku definicji kolorowania kodu, "" gdy brak</summary>
        public string HighlightingPatternPath
        {
            get { return (string)GetValue(HighlightingPatternPathProperty); }
            set { SetValue(HighlightingPatternPathProperty, value); }
        }
        public static readonly DependencyProperty HighlightingPatternPathProperty =
            DependencyProperty.Register("HighlightingPatternPath", typeof(string), typeof(CodeEditor), new PropertyMetadata(""));


        /// <summary>Kod programu</summary>
        public string ProgramText
        {
            get { return (string)GetValue(ProgramTextProperty); }
            set { SetValue(ProgramTextProperty, value); }
        }
        public static readonly DependencyProperty ProgramTextProperty =
            DependencyProperty.Register(
                "ProgramText",
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata("", ProgramTextPropertyChangedCallback)
                );


        /// <summary>Zmienna puli podpowiedzi dla edytora kodu</summary>
        public HintPool? HintPool
        {
            get { return (HintPool?)GetValue(HintPoolProperty); }
            set { SetValue(HintPoolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintPool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintPoolProperty =
            DependencyProperty.Register("HintPool", typeof(HintPool), typeof(CodeEditor), new PropertyMetadata(null));


        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Control dependency properties callbacks space below ↓

        /// <summary>
        /// Funkcja zmienia obecnie podświetlaną linię kodu oraz nadzoruje bolkadę wyświetlania
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CurrentlyExecutedLineOfCodePropertyCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            int lineToHighlight = (int)e.NewValue;
            CodeEditor editor = (CodeEditor)sender;
            editor.lineHighlightRenderer.LineToHighlight = lineToHighlight;
            editor.textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            if (lineToHighlight == -1)
            {
                editor.textEditor.IsReadOnly = false;
                editor.codeExecutionProgressBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                editor.textEditor.IsReadOnly = true;
                editor.codeExecutionProgressBar.Visibility = Visibility.Visible;
            }
        }

        public static void ProgramTextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            string newCode = (string)e.NewValue;
            CodeEditor editor = (CodeEditor)sender;
            // Porównanie wywoływane ze względu na ustawianą wartość w OnTextChanged
            // Jeżeli ta sama wartość to adres ten sam i nie wymaga zmian
            // Wywoływane w celu uniknięcia zapętlenia
            if (ReferenceEquals(newCode, editor.textEditor.Text))
                return;
            editor.textEditor.Text = newCode;
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Event functions space here ↓

        private void OnTextChanged(object? sender, EventArgs e)
        {
            ProgramText = textEditor.Text;
        }

        /// <summary>
        /// Funkcja wywoływana przy zmianie wprowadzonego tekstu.
        /// Odpowiada za podpowiadanie użytkownikowi komend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Sprawdzanie czy użytkownik ma włączoną opcję podpowiedzi
            if (!ShowCodingHints)
                return;
            // Sprawdzanie czy ustawiono pulę podpoiwedzi
            if (HintPool == null)
                return;

            // Pobieranie obecnej lini tekstu
            int caretOffset = textEditor.CaretOffset;
            ICSharpCode.AvalonEdit.Document.DocumentLine line = textEditor.Document.GetLineByOffset(caretOffset);
            string textLine = textEditor.Document.GetText(line.Offset, line.Length);

            // Tworzenie okna i puli podpowiedzi
            CompletionWindow completionWindow = new CompletionWindow(textEditor.TextArea);

            // Tworzenie tablicy podpowiedzi
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            // Usuwanie białych znaków
            textLine = textLine.Trim();

            // Sprawdzanie czy użytkownik nacisną lub wprowadził kropkę - użytkownik wprowadza komendę
            if (e.Text == "." || textLine.Contains('.'))
            {
                // Znajdywanie odpowiedniej grupy podpowiedzi
                foreach (string commandGroup in HintPool.GroupsList)
                    if (textLine.Contains(commandGroup))
                    {
                        // Znajdywanie pasujących komend
                        string commandPart = textLine.Substring(textLine.IndexOf('.') + 1);
                        foreach (MyCompletionData elem in HintPool.GetCommands(commandGroup))
                        {
                            /*
                             * Sprawdzenie warunków podania podpowiedzi komendy:
                             *      1. Czy użytkownik nie napisał już całej komendy.
                             *      2. Czy wprowadzany tekst jest początkiem komendy.
                             */
                            if(!elem.Text.Equals(commandPart) && elem.Text.Length > commandPart.Length && elem.Text.Substring(0, commandPart.Length).Equals(commandPart))
                                data.Add(elem);
                        }
                    }
            }
            else
            {
                // Generowanie podpowiedzi dla grupy komend
                foreach (string commandGroup in HintPool.GroupsList)
                {
                    /*
                     * Sprawdzenie trzech warunków podania podpowiedzi słowa kluczowego:
                     *      1. Czy wprowadzony znak nie był znakiem pustym np. Enter, Tab, ... . Operacja wykonana przez sprawdzenie długości wczytanej lini.
                     *      2. Czy użytkownik nie napisał już całego słowa kluczowego.
                     *      3. Czy wprowadzany tekst jest początkiem słowa kluczowego.
                     *      
                     */
                    if (textLine.Length != 0 && commandGroup.Length > textLine.Length && commandGroup.Substring(0, textLine.Length).Equals(textLine))
                        data.Add(HintPool.GetGroupCompletionData(commandGroup));
                }
            }

            // Wyświetlenie okna podpowiedzi jeżeli lista podpowiedzi nie jest pusta
            if (data.Count > 0)
            {
                completionWindow.Show();
                /*completionWindow.Closed += delegate {
                    completionWindow = null;
                };*/
            }
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Public functions (control interface) space below ↓

        public void insertTextOnCaret(string text_) =>
            textEditor.Document.Insert(textEditor.CaretOffset, text_);

        public void insertText(string text_, int offset_) =>
            textEditor.Document.Insert(offset_, text_);

        public string getText() =>
            textEditor.Text;

        public void setText(string text_) =>
            textEditor.Text = text_;

        public int getCaretOffset() =>
            textEditor.CaretOffset;

        public void setCaretOffset(int offest_) =>
            textEditor.CaretOffset = offest_;

        public int getCurrentLineNumber()
        {
            int caretOffset = textEditor.CaretOffset;
            ICSharpCode.AvalonEdit.Document.DocumentLine line = textEditor.Document.GetLineByOffset(caretOffset);
            return line.Offset;
        }
        public string getCurrentLineText()
        {
            int caretOffset = textEditor.CaretOffset;
            ICSharpCode.AvalonEdit.Document.DocumentLine line = textEditor.Document.GetLineByOffset(caretOffset);
            return textEditor.Document.GetText(line.Offset, line.Length);
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Click functions space below ↓

        private void ShowBlankCharactersToggleButtonClick(object sender, RoutedEventArgs e)
        {
            textEditor.Options.ShowEndOfLine = BlankCharactersVisibility;
            textEditor.Options.ShowTabs = BlankCharactersVisibility;
        }

        private void TextWrapToggleButtonClick(object sender, RoutedEventArgs e)
        {
            textEditor.WordWrap = TextWraping;
        }

        private void ShowLineNumberingToggleButtonClick(object sender, RoutedEventArgs e)
        {
            textEditor.ShowLineNumbers = LineNumbering;
        }

        private void FontSizeComboboxSelectionChanged(object sender, EventArgs e)
        {
            double newFontSize = double.Parse(SelectedFontSize);
            if (newFontSize != textEditor.FontSize)
                textEditor.FontSize = newFontSize;
        }

        private void FontTypeComboboxSelectionChanged(object sender, EventArgs e)
        {
            FontFamily newFontType = new FontFamily(SelectedFontType);
            if (!newFontType.Equals(textEditor.FontFamily))
                textEditor.FontFamily = newFontType;
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // ObservablePropertys space below ↓
        public List<string> FontSizes
        {
            get => this.fontSizes;
            set
            {
                this.fontSizes = value;
                OnPropertyChanged("FontSizes");
            }
        }
        private List<string> fontSizes = new List<string> { "8", "9", "10", "11", "12", "14", "16", "20", "24", "28", "32", "36" };

        public string SelectedFontSize
        {
            get => this.selectedFontSize;
            set
            {
                this.selectedFontSize = value;
                OnPropertyChanged("SelectedFontSize");
            }
        }
        private string selectedFontSize = "14";

        public List<string> FontTypes
        {
            get => this.fontTypes;
            set
            {
                this.fontTypes = value;
                OnPropertyChanged("FontTypes");
            }
        }
        private List<string> fontTypes = new List<string> { };

        public string SelectedFontType
        {
            get => this.selectedFontType;
            set
            {
                this.selectedFontType = value;
                OnPropertyChanged("SelectedFontType");
            }
        }
        private string selectedFontType = "";

        public bool BlankCharactersVisibility
        {
            get => this.blankCharactersVisibility;
            set
            {
                this.blankCharactersVisibility = value;
                OnPropertyChanged("BlankCharactersVisibility");
            }
        }
        private bool blankCharactersVisibility = true;

        public bool TextWraping
        {
            get => this.textWraping;
            set
            {
                this.textWraping = value;
                OnPropertyChanged("TextWraping");
            }
        }
        private bool textWraping = true;

        public bool LineNumbering
        {
            get => this.lineNumbering;
            set
            {
                this.lineNumbering = value;
                OnPropertyChanged("LineNumbering");
            }
        }
        private bool lineNumbering = true;

        public bool ShowCodingHints
        {
            get => this.showCodingHints;
            set
            {
                this.showCodingHints = value;
                OnPropertyChanged("ShowCodingHints");
            }
        }
        private bool showCodingHints = true;
    }
}
