using System;
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

using System.ComponentModel;

namespace LabControlsWPF.AutoPid
{
    /// <summary>
    /// Logika interakcji dla klasy AutoPidMenuControl.xaml
    /// </summary>
    public partial class AutoPidMenuControl : UserControl, INotifyPropertyChanged
    {
        private const int _maxAutoPidPointCount = 50;
        private List<PidInputBox> _pidInputBoxes = new List<PidInputBox>();
        private bool _BrutalLock { get; set; } = false;
        private AutoPidPool MainPidPool = new AutoPidPool();
        public AutoPidMenuControl()
        {
            InitializeComponent();
            this.Loaded += UserControlLoaded;
        }

        /// <summary>Implementacja eventu INotifyPropertyChanged</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Implementacja funkcji wywołania INotifyPropertyChanged</summary>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Funkcja uruchamiana po załadowaniu parametrów DependencyProperity do kontrolki oraz zakończeniu jej tworzenia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            GenerateAutoPidFields(int.Parse(SelectedAutoPidPointCount));
        }

        /// <summary>
        /// Funkcja generuje pola wprowadzania pid zgodnie z wybraną ilością pól.
        /// Dodatkowo ustawia nową ilość pól oraz ich wyjście w danych wyjściowych PidPool.
        /// Użycie jej jest stosunkowo ciężką obliczeniowo operacją.
        /// </summary>
        /// <param name="firstRun">Czy jest to pierwsze wywołanie(nie trzeba czyścić GUI i eventów)</param>
        /// <param name="clearOutputData">Czy tworzyć nowe dane wyjściowe(nie przy odświeżeniu wywołanym zmianą modelu)</param>
        private void GenerateAutoPidFields(int newInputCount)
        {
            // Sprawdzanie wywołania które nic nie zmienia
            if (newInputCount == _pidInputBoxes.Count)
                return;
            // Zwalnianie zasobów kontrolek
            foreach (PidInputBox input in _pidInputBoxes)
                input.PidChanged -= AutoPidDataUpdateCallback;
            _pidInputBoxes.Clear();
            ParameterStackPanel.Children.Clear();

            // Dodawanie nowych kontrolek
            _pidInputBoxes = new List<PidInputBox>(newInputCount);
            for(int i = 0; i < newInputCount; i++)
            {
                PidInputBox newInput = new PidInputBox(i);
                _pidInputBoxes.Add(newInput);
                newInput.PidChanged += AutoPidDataUpdateCallback;
                ParameterStackPanel.Children.Add(newInput);
            }

            // Generowanie nowych pól zapisu danych
            MainPidPool.StrPidList = new List<Tuple<string, string, string, string>>();
            for (int i = 0; i < newInputCount; i++)
            {
                MainPidPool.StrPidList.Add(new Tuple<string, string, string, string>("", "", "", ""));
            }
        }

        /// <summary>
        /// Funkcja jest wywoływana za każdym razem gdy zostanie wywołany event zmiany danych auto pid kontrolki PidInputBox.
        /// Odpowiada za ustawienie danych zwracanych przez kontrolkę.
        /// </summary>
        /// <param name="obj">Kontrolka[PidInputBox]</param>
        /// <param name="inputID">ID kontrolki</param>
        private void AutoPidDataUpdateCallback(object? sender, int inputID)
        {
            if (_BrutalLock)
                return;
            if (sender == null) // To się nie powinno zdażyć
                throw new ArgumentNullException("AutoPidMenuControl.AutoPidDataUpdateCallback-Null sender instead of PidInputBox");
            PidInputBox input = (PidInputBox)sender;
            this.MainPidPool.StrPidList[inputID] = new Tuple<string, string, string, string>(
                input.TemperatureValue,
                input.ParameterPValue,
                input.ParameterIValue,
                input.ParameterDValue
                );
        }

        /// <summary>
        /// Funkcja odświeża zawartość domyślnego nastawu pid na wyjściu PidPool.
        /// </summary>
        private void UpdateDefaultPidOutput(string? newP = null, string? newI = null, string? newD = null)
        {
            if (_BrutalLock)
                return;
            MainPidPool.StrDefaultPidValue = new Tuple<string, string, string>(
                newP ?? DefaultParameterPValue,
                newI ?? DefaultParameterIValue,
                newD ?? DefaultParameterDValue
                );
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Control dependency properties space below ↓

        /// <summary>Czy autopid jest aktywny</summary>



        public bool IsAutoPid
        {
            get { return (bool)GetValue(IsAutoPidProperty); }
            set { SetValue(IsAutoPidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAutoPid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAutoPidProperty =
            DependencyProperty.Register("IsAutoPid", typeof(bool), typeof(AutoPidMenuControl), new PropertyMetadata(false));



        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Event functions space here ↓



        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Public functions (control interface) space below ↓

        public void SetPidPool(AutoPidPool newPool)
        {
            _BrutalLock = true;
            SelectedAutoPidPointCount = newPool.SelectedAutoPidPointCount;
            DefaultParameterPValue = newPool.StrDefaultPidValue.Item1;
            DefaultParameterIValue = newPool.StrDefaultPidValue.Item2;
            DefaultParameterDValue = newPool.StrDefaultPidValue.Item3;
            MainPidPool.StrPidList = newPool.StrPidList;

            // Wpisanie danych do inputów
            for (int i = 0; i < int.Parse(newPool.SelectedAutoPidPointCount); i++)
            {
                this._pidInputBoxes[i].TemperatureValue = newPool.StrPidList[i].Item1;
                this._pidInputBoxes[i].ParameterPValue = newPool.StrPidList[i].Item2;
                this._pidInputBoxes[i].ParameterIValue = newPool.StrPidList[i].Item3;
                this._pidInputBoxes[i].ParameterDValue = newPool.StrPidList[i].Item4;
            }
            _BrutalLock = false;
        }

        public AutoPidPool GetPidPool()
        {
            // Odświeżanie danych modelu
            UpdateDefaultPidOutput();
            for (int i = 0; i < int.Parse(SelectedAutoPidPointCount); i++)
            {
                MainPidPool.StrPidList[i] = new Tuple<string, string, string, string>(
                    this._pidInputBoxes[i].TemperatureValue,
                    this._pidInputBoxes[i].ParameterPValue,
                    this._pidInputBoxes[i].ParameterIValue,
                    this._pidInputBoxes[i].ParameterDValue
                    );
            }
            return MainPidPool;
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Click functions space below ↓





        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // ObservablePropertys space below ↓

        public List<string> AutoPidPointCountList
        {
            get => this.autoPidPointCountList;
            set
            {
                this.autoPidPointCountList = value;
                OnPropertyChanged("AutoPidPointCountList");
            }
        }
        private List<string> autoPidPointCountList =
            Enumerable.Range(1, _maxAutoPidPointCount)
            .Select(i => i.ToString())
            .ToList();

        public string SelectedAutoPidPointCount
        {
            get => selectedAutoPidPointCount;
            set
            {
                selectedAutoPidPointCount = value;
                MainPidPool.SelectedAutoPidPointCount = value;
                GenerateAutoPidFields(int.Parse(value));
                OnPropertyChanged("SelectedAutoPidPointCount");
            }
        }
        private string selectedAutoPidPointCount = "1";

        public string DefaultParameterPValue
        {
            get => this.defaultParameterPValue;
            set
            {
                this.defaultParameterPValue = value;
                UpdateDefaultPidOutput();
                OnPropertyChanged("DefaultParameterPValue");
            }
        }
        private string defaultParameterPValue = "150";

        public string DefaultParameterIValue
        {
            get => this.defaultParameterIValue;
            set
            {
                this.defaultParameterIValue = value;
                UpdateDefaultPidOutput();
                OnPropertyChanged("DefaultParameterIValue");
            }
        }
        private string defaultParameterIValue = "50";

        public string DefaultParameterDValue
        {
            get => this.defaultParameterDValue;
            set
            {
                this.defaultParameterDValue = value;
                UpdateDefaultPidOutput();
                OnPropertyChanged("DefaultParameterDValue");
            }
        }
        private string defaultParameterDValue = "1";

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            bool? choice = MaterialMessageBox.NewFastMessage(MaterialMessageFastType.ConfirmActionInfo, "Czy na pewno chcesz zresetować ustawienia auto pid?", true);
            if (choice.HasValue && choice.Value == true)
                SetPidPool(new AutoPidPool("150", "50", "1", "1"));
        }
    }
}
