using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezerGUI.ViewModels
{
    /// <summary>
    /// Szablon do tworzenia ViewModel
    /// 
    /// Schemat dodawania do okna:
    /// this.FontSize = BootStrapper.settings.GlobalFontSize;
    /// this.DataContext = new TemplateViewModel();
    /// Closing += (sender, args) => ((TemplateViewModel)this.DataContext).Dispose();
    /// </summary>
    public partial class TemplateViewModel : ObservableObject, IDisposable
    {
        public TemplateViewModel()
        {
            // Ustawianie wartości parametrów
            InitializeDefaultValues();

            // Podłączanie eventów

            // Inicjalizowanie komend
        }

        public void InitializeDefaultValues()
        {
            ;
        }

        // Pola
        // --------------------------------------------------

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
