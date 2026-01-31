using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezerGUI.ViewModels
{
    public partial class MainWindowVM : ObservableObject, IDisposable
    {
        public MainWindowVM()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów

            // Inicjalizowanie komend
        }

        public void InitializeDefaultValues()
        {
            IsFlowSensorViewEnabled = BootStrapper.FlowSensorController.IsActive();
        }

        // Pola
        // --------------------------------------------------

        [ObservableProperty]
        private bool isFlowSensorViewEnabled;

        // Funkcje komend
        // --------------------------------------------------
        // MaterialMessageBox.NewFastMessage(MaterialMessageFastType.NotImplementedWarning, $"Brak implementacji komendy: '{new StackTrace().GetFrame(0)?.GetMethod()?.Name}'");

        // Funkcje eventów
        // --------------------------------------------------

        // Funkcje walidacji danych
        // --------------------------------------------------

        // Inne funkcje
        // --------------------------------------------------

        // Implementacje interfejsów (np. IDisposable)
        // --------------------------------------------------

        public void Dispose()
        {
            // Odłączanie eventów
            ;
        }
    }
}
