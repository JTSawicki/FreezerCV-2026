using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreezerGUI.Windows;
using LabControlsWPF;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace FreezerGUI.ViewModels
{
    /// <summary>
    /// ViewModel dla okna ustawień
    /// </summary>
    public partial class SettingsWindowVM : ObservableObject, IDisposable
    {
        private readonly PaletteHelper _paletteHelper = new();

        public SettingsWindowVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów

            // Inicjalizowanie komend
            SelectDefaultSaveFolderCommand = new RelayCommand(SelectDefaultSaveFolder);
            LoadDefaultCommand = new RelayCommand(LoadDefault);
            ResetChangesCommand = new RelayCommand(ResetChanges);
            SaveCommand = new RelayCommand<Window>(Save);
        }

        public void InitializeDefaultValues()
        {
            SelectedKithleyAdress = BootStrapper.Settings.DefaultKithleyAdress;
            SelectedLakeShoreAdress = BootStrapper.Settings.DefaultLakeShoreAdress;
            DefaultSaveFolder = BootStrapper.Settings.DefaultSaveFolder;
            SaveLastAsDefaultSaveFolder = BootStrapper.Settings.SaveLastAsDefaultSaveFolder;

            ColorList = new ObservableCollection<ColorData>
            {
                new ColorData("Kolor główny",   (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.PrimaryColor)),
                new ColorData("Kolor akcentu",  (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.SecondaryColor)),
                new ColorData("Kolor tekstu 1", (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.PrimaryTextColor)),
                new ColorData("Kolor tekstu 2", (Color)ColorConverter.ConvertFromString(BootStrapper.Settings.SecondaryTextColor)),
            };
            SelectedColorIndex = 0;
            PickedColor = ColorList[SelectedColorIndex].ColorValue;

            TextColorEnforcement = BootStrapper.Settings.TextColorEnforcement;
            DarkMode = BootStrapper.Settings.DarkMode;

            SelectedFontSize = BootStrapper.Settings.GlobalFontSize.ToString();
        }

        // Pola
        // --------------------------------------------------

        public IList<string> KithleyAdressList => Enumerable.Range(Constants.GpibMinAddress, Constants.GpibMaxAddress)
            .Select(x => x.ToString())
            .Where(x => !x.Equals(SelectedLakeShoreAdress) && !x.Equals(Constants.GpibControllerAddress.ToString()))
            .ToList();
        [ObservableProperty, NotifyPropertyChangedFor(nameof(LakeShoreAdressList))]
        private string selectedKithleyAdress;

        public IList<string> LakeShoreAdressList => Enumerable.Range(Constants.GpibMinAddress, Constants.GpibMaxAddress)
            .Select(x => x.ToString())
            .Where(x => !x.Equals(SelectedKithleyAdress) && !x.Equals(Constants.GpibControllerAddress.ToString()))
            .ToList();
        [ObservableProperty, NotifyPropertyChangedFor(nameof(KithleyAdressList))]
        private string selectedLakeShoreAdress;

        [ObservableProperty]
        private string defaultSaveFolder;
        [ObservableProperty]
        private bool saveLastAsDefaultSaveFolder;

        [ObservableProperty]
        private IEnumerable<string> fontSizeList = Enumerable.Range(5, 40)
            .Select(x => x.ToString());
        [ObservableProperty]
        private string selectedFontSize;

        public ObservableCollection<ColorData> ColorList { get; private set; }
        [ObservableProperty]
        private int selectedColorIndex;

        public bool TextColorEnforcement
        {
            get => _textColorEnforcement;
            set
            {
                SetProperty(ref _textColorEnforcement, value);
                ApplayTheme();
            }
        }
        public bool _textColorEnforcement;
        
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                SetProperty(ref _darkMode, value);
                ApplayTheme();
            }
        }
        public bool _darkMode;

        public Color PickedColor
        {
            get => _pickedColor;
            set
            {
                SetProperty(ref _pickedColor, value);

                ColorData color = ColorList[SelectedColorIndex];
                int indexBuffer = SelectedColorIndex;
                color.ColorValue = value;
                ColorList.RemoveAt(indexBuffer);
                ColorList.Insert(indexBuffer, color);
                SelectedColorIndex = indexBuffer;

                ApplayTheme();
            }
        }
        private Color _pickedColor;

        public RelayCommand SelectDefaultSaveFolderCommand { get; }
        public RelayCommand ResetChangesCommand { get; }
        public RelayCommand LoadDefaultCommand { get; }
        public RelayCommand<Window> SaveCommand { get; }

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        private void SelectDefaultSaveFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                DefaultSaveFolder = dialog.SelectedPath;
            }
        }

        private void Save(Window? window)
        {
            // Sprawdzenie czy podano poprawny folder
            if (!Directory.Exists(DefaultSaveFolder))
            {
                MaterialMessageBox.NewFastMessage(MaterialMessageFastType.BadUserInputWarning, "Podany domyślny folder zapisu nie istnieje");
                return;
            }

            BootStrapper.Settings.DefaultKithleyAdress = SelectedKithleyAdress;
            BootStrapper.Settings.DefaultLakeShoreAdress = SelectedLakeShoreAdress;
            BootStrapper.Settings.DefaultSaveFolder = DefaultSaveFolder;
            BootStrapper.Settings.SaveLastAsDefaultSaveFolder = SaveLastAsDefaultSaveFolder;
            BootStrapper.Settings.PrimaryColor = ColorList[0].ColorValue.ToString();
            BootStrapper.Settings.SecondaryColor = ColorList[1].ColorValue.ToString();
            BootStrapper.Settings.PrimaryTextColor = ColorList[2].ColorValue.ToString();
            BootStrapper.Settings.SecondaryTextColor = ColorList[3].ColorValue.ToString();
            BootStrapper.Settings.TextColorEnforcement = TextColorEnforcement;
            BootStrapper.Settings.DarkMode = DarkMode;
            BootStrapper.Settings.GlobalFontSize = double.Parse(SelectedFontSize);

            BootStrapper.Settings.Save();

            // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, "Zapisano zmiany ustawień");
            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.Show();
            window!.Close();
        }

        private void ResetChanges()
        {
            InitializeDefaultValues();
        }

        private void LoadDefault()
        {
            AppSettings defaultSettings = AppSettings.InitializeDefault();
            SelectedKithleyAdress = defaultSettings.DefaultKithleyAdress;
            SelectedLakeShoreAdress = defaultSettings.DefaultLakeShoreAdress;
            DefaultSaveFolder = defaultSettings.DefaultSaveFolder;

            ColorList = new ObservableCollection<ColorData>
            {
                new ColorData("Kolor główny",   (Color)ColorConverter.ConvertFromString(defaultSettings.PrimaryColor)),
                new ColorData("Kolor akcentu",  (Color)ColorConverter.ConvertFromString(defaultSettings.SecondaryColor)),
                new ColorData("Kolor tekstu 1", (Color)ColorConverter.ConvertFromString(defaultSettings.PrimaryTextColor)),
                new ColorData("Kolor tekstu 2", (Color)ColorConverter.ConvertFromString(defaultSettings.SecondaryTextColor)),
            };
            SelectedColorIndex = 0;
            PickedColor = ColorList[SelectedColorIndex].ColorValue;

            TextColorEnforcement = defaultSettings.TextColorEnforcement;
            DarkMode = defaultSettings.DarkMode;
        }

        // Funkcje eventów
        // --------------------------------------------------

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        /// <summary>
        /// Ustawia wybrany schemat kolorów
        /// </summary>
        private void ApplayTheme()
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(ColorList[0].ColorValue);
            theme.SetSecondaryColor(ColorList[1].ColorValue);

            if(TextColorEnforcement)
            {
                theme.PrimaryLight = new ColorPair(theme.PrimaryLight.Color, ColorList[2].ColorValue);
                theme.PrimaryMid = new ColorPair(theme.PrimaryMid.Color, ColorList[2].ColorValue);
                theme.PrimaryDark = new ColorPair(theme.PrimaryDark.Color, ColorList[2].ColorValue);
                theme.SecondaryLight = new ColorPair(theme.SecondaryLight.Color, ColorList[3].ColorValue);
                theme.SecondaryMid = new ColorPair(theme.SecondaryMid.Color, ColorList[3].ColorValue);
                theme.SecondaryDark = new ColorPair(theme.SecondaryDark.Color, ColorList[3].ColorValue);
            }

            if (DarkMode)
                theme.SetBaseTheme(Theme.Dark);
            else
                theme.SetBaseTheme(Theme.Light);

            paletteHelper.SetTheme(theme);
        }

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            ;
        }

        public class ColorData
        {
            public string ColorName { get; set; }
            public Color ColorValue { get; set; }

            public ColorData(string colorName, Color colorBrush)
            {
                ColorName = colorName;
                ColorValue = colorBrush;
            }
        }
    }
}
