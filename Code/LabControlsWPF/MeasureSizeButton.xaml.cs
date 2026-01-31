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

namespace LabControlsWPF
{
    /// <summary>
    /// Logika interakcji dla klasy MeasureSizeButton.xaml
    /// </summary>
    public partial class MeasureSizeButton : UserControl
    {


        public bool SendMessage
        {
            get { return (bool)GetValue(SendMessageProperty); }
            set { SetValue(SendMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SendMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendMessageProperty =
            DependencyProperty.Register("SendMessage", typeof(bool), typeof(MeasureSizeButton), new PropertyMetadata(false));



        public MeasureSizeButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double height = MasterPanel.ActualHeight;
            double width = MasterPanel.ActualWidth;
            string NewButtonContent = $"Zmierz rozmiar okna\nWysokość: {height}\nSzerokość: {width}";
            MeasurementButton.Content = NewButtonContent;
            if (SendMessage)
                MaterialMessageBox.NewFastMessage(MaterialMessageFastType.Information, $"Rozmiar elementu\nWysokość: {height}\nSzerokość: {width}");
        }

        private void MasterPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MeasurementButton.Content = "Zmierz rozmiar okna\nWysokość: -\nSzerokość: -";
        }
    }
}
