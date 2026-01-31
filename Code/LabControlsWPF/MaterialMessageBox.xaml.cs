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
using System.Windows.Shapes;

namespace LabControlsWPF
{
    /// <summary>
    /// Logika interakcji dla klasy MaterialMessageBox.xaml
    /// </summary>
    public partial class MaterialMessageBox : Window
    {
        private readonly bool _IsBlocking;

        private MaterialMessageBox(string message, MaterialMessageBoxType type, MaterialMessageBoxButtons buttons, bool isBlocking, string messageBoxTitle = "default")
        {
            InitializeComponent();

            this.MessageText.Text = message;
            _IsBlocking = isBlocking;

            SetButtonsVisibility(buttons);
            SetImage(type);
            SetWindowTitle(messageBoxTitle, type);
        }

        // Funkcje ustawiające parametry okna
        private void SetButtonsVisibility(MaterialMessageBoxButtons buttons)
        {
            if (buttons == MaterialMessageBoxButtons.Ok)
            {
                YesButton.Visibility = Visibility.Collapsed;
                NoButton.Visibility = Visibility.Collapsed;
            }
            if (buttons == MaterialMessageBoxButtons.YesNo)
            {
                OkButton.Visibility = Visibility.Collapsed;
            }
        }
        private void SetImage(MaterialMessageBoxType type)
        {
            if(type == MaterialMessageBoxType.Confirmation)
            {
                ImageViewer.Source = new BitmapImage(new Uri("pack://siteOfOrigin:,,,/LabControlsWPFResources/MaterialMessageBox/check-mark.png"));
            }
            if (type == MaterialMessageBoxType.Error)
            {
                ImageViewer.Source = new BitmapImage(new Uri("pack://siteOfOrigin:,,,/LabControlsWPFResources/MaterialMessageBox/error.png"));
            }
            if (type == MaterialMessageBoxType.Info)
            {
                ImageViewer.Source = new BitmapImage(new Uri("pack://siteOfOrigin:,,,/LabControlsWPFResources/MaterialMessageBox/info.png"));
            }
            if (type == MaterialMessageBoxType.Warning)
            {
                ImageViewer.Source = new BitmapImage(new Uri("pack://siteOfOrigin:,,,/LabControlsWPFResources/MaterialMessageBox/warning.png"));
            }
        }
        private void SetWindowTitle(string messageBoxTitle, MaterialMessageBoxType type)
        {
            if ( ! string.IsNullOrEmpty(messageBoxTitle) || 
                messageBoxTitle != "default" )
            {
                this.Title = messageBoxTitle;
                return;
            }
            if (type == MaterialMessageBoxType.Confirmation)
            {
                this.Title = "Confirmation";
                return;
            }
            if (type == MaterialMessageBoxType.Error)
            {
                this.Title = "Error";
                return;
            }
            if (type == MaterialMessageBoxType.Info)
            {
                this.Title = "Info";
                return;
            }
            if (type == MaterialMessageBoxType.Warning)
            {
                this.Title = "Warning";
                return;
            }
        }

        // Funkcje przycisków okna dialogowego. Odpowiadają one za zwracane wartości
        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(true, true);
        }
        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(false, true);
        }
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(true, true);
        }

        // Funkcja zwrotu odpowiedzi okna dialogowego
        private void SetDialogResult(bool? result, bool close = false)
        {
            if (_IsBlocking) this.DialogResult = result;
            if(close) this.Close();
        }

        /// <summary>
        /// Uruchamia okno powiadomienia zwracające informację true/false.
        /// Blokuje działąnie interfejsu.
        /// </summary>
        /// <param name="message">Wiadomość wyświetlana</param>
        /// <param name="type">Typ okna</param>
        /// <param name="buttons">Typ wyświetlanych przycisków</param>
        /// <param name="messageBoxTitle">Tytuł okna. Dla wartości domyślnej taki jak typ okna</param>
        /// <returns></returns>
        public static bool? NewBlockingMessage(string message, MaterialMessageBoxType type, MaterialMessageBoxButtons buttons, string messageBoxTitle = "default")
        {
            MaterialMessageBox messageBox = new MaterialMessageBox(message, type, buttons, true, messageBoxTitle);
            return messageBox.ShowDialog();
        }
        /// <summary>
        /// Uruchamia okno powiadomienia.
        /// Jest ono informacyjne i nie zwraca odpowiedzi.
        /// </summary>
        /// <param name="message">Wiadomość wyświetlana</param>
        /// <param name="type">Typ okna</param>
        /// <param name="messageBoxTitle">Tytuł okna. Dla wartości domyślnej taki jak typ okna</param>
        public static void NewNonBlockingMessage(string message, MaterialMessageBoxType type, string messageBoxTitle = "default")
        {
            MaterialMessageBox messageBox = new MaterialMessageBox(message, type, MaterialMessageBoxButtons.Ok, false, messageBoxTitle);
            messageBox.Show();
        }

        /// <summary>
        /// Funkcja pozwala wykonać akcję szybkiego okna powiadomień w programie bez specyfikowania wszystkich elementów.
        /// </summary>
        /// <param name="type">Typ szybkiego powiadomienia</param>
        /// <param name="message">Element którego dotyczy powiadomienie</param>
        /// <param name="isBlocking">Czy okno zwraca odpowiedź</param>
        /// <returns>Domyślnie null lub true/false/null jeżeli okno zwracające</returns>
        public static bool? NewFastMessage(MaterialMessageFastType type, string message = "", bool isBlocking = false)
        {
            MaterialMessageBox messageBox;

            if(type == MaterialMessageFastType.BadUserInputWarning)
            {
                messageBox = new MaterialMessageBox(
                    message: $"Nieprawidłowe wejście wprowadzone przez użytkownika.\n{message}",
                    type: MaterialMessageBoxType.Warning,
                    buttons: isBlocking == true ? MaterialMessageBoxButtons.YesNo : MaterialMessageBoxButtons.Ok,
                    isBlocking: isBlocking,
                    messageBoxTitle: "BadUserInput"
                    );
            }
            else if (type == MaterialMessageFastType.ConfirmActionInfo)
            {
                messageBox = new MaterialMessageBox(
                    message: $"Proszę potwierdzić akcję.\n{message}",
                    type: MaterialMessageBoxType.Confirmation,
                    buttons: isBlocking == true ? MaterialMessageBoxButtons.YesNo : MaterialMessageBoxButtons.Ok,
                    isBlocking: isBlocking,
                    messageBoxTitle: "ConfirmAction"
                    );
            }
            else if (type == MaterialMessageFastType.Information)
            {
                messageBox = new MaterialMessageBox(
                    message: $"{message}",
                    type: MaterialMessageBoxType.Info,
                    buttons: isBlocking == true ? MaterialMessageBoxButtons.YesNo : MaterialMessageBoxButtons.Ok,
                    isBlocking: isBlocking,
                    messageBoxTitle: "Information"
                    );
            }
            else if (type == MaterialMessageFastType.InternalError)
            {
                messageBox = new MaterialMessageBox(
                    message: $"Wewnętrzny błąd programu.\n{message}",
                    type: MaterialMessageBoxType.Error,
                    buttons: isBlocking == true ? MaterialMessageBoxButtons.YesNo : MaterialMessageBoxButtons.Ok,
                    isBlocking: isBlocking,
                    messageBoxTitle: "InternalError"
                    );
            }
            else if (type == MaterialMessageFastType.NotImplementedWarning)
            {
                messageBox = new MaterialMessageBox(
                    message: $"Brak implementacji elementu.\n{message}",
                    type: MaterialMessageBoxType.Warning,
                    buttons: isBlocking == true ? MaterialMessageBoxButtons.YesNo : MaterialMessageBoxButtons.Ok,
                    isBlocking: isBlocking,
                    messageBoxTitle: "NotImplementedError"
                    );
            }
            else
            {
                messageBox = new MaterialMessageBox(
                    message: $"Brak implementacjiszybkiego okna powiadomień o podanym typie XD",
                    type: MaterialMessageBoxType.Error,
                    buttons: MaterialMessageBoxButtons.Ok,
                    isBlocking: false,
                    messageBoxTitle: "FastMessageError"
                    );
            }
            if (isBlocking)
                return messageBox.ShowDialog();
            else
            {
                messageBox.Show();
                return null;
            }
        }
    }

    /// <summary>
    /// Wyliczenie służące do określania widocznych przycisków
    /// </summary>
    public enum MaterialMessageBoxButtons
    {
        /// <summary> Dwa przyciski Yes + No. Zwraca odpowiedź true lub flase. </summary>
        YesNo,
        /// <summary> Jeden przycisk OK. Zwraca true. </summary>
        Ok
    }

    /// <summary>
    /// Wyliczenie służące do określania typu powiadomienia
    /// </summary>
    public enum MaterialMessageBoxType
    {
        /// <summary> Okno potwierdzenia </summary>
        Confirmation,
        /// <summary> Okno błędu</summary>
        Error,
        /// <summary> Okno informacyjne </summary>
        Info,
        /// <summary> Okno ostrzeżenia</summary>
        Warning,
        
    }

    /// <summary>
    /// Wyliczenie służące do określania typu szybkiego powiadomienia
    /// </summary>
    public enum MaterialMessageFastType
    {
        /// <summary> Błąd nieprawidłowego wejścia podanego przez użytkownika</summary>
        BadUserInputWarning,
        /// <summary> Potwierdzenie akcji </summary>
        ConfirmActionInfo,
        /// <summary> Szybka informacja bez potwierdzenia </summary>
        Information,
        /// <summary> Wewnętrzny błąd programu </summary>
        InternalError,
        /// <summary> Błąd braku implementacji </summary>
        NotImplementedWarning
    }
}
