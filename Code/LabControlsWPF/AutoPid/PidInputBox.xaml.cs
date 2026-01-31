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
    /// Logika interakcji dla klasy PidInputBox.xaml
    /// Jest to kontrolka inputu jednego zestawu parametrów auto pid
    /// </summary>
    public partial class PidInputBox : UserControl
    {
        /// <summary>Event wywoływany przy zmianie parametrów</summary>
        public event EventHandler<int>? PidChanged;
        /// <summary>Identyfikator kontrolki</summary>
        public int ControlID { get; private set; }
        public PidInputBox(int controlID)
        {
            InitializeComponent();
            ControlID = controlID;
        }

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Control dependency properties space below ↓

        public string TemperatureValue
        {
            get { return (string)GetValue(TemperatureValueProperty); }
            set { SetValue(TemperatureValueProperty, value); }
        }
        public static readonly DependencyProperty TemperatureValueProperty =
            DependencyProperty.Register("TemperatureValue", typeof(string), typeof(PidInputBox), new PropertyMetadata("", AnyParameterChangedCallback));

        public string ParameterPValue
        {
            get { return (string)GetValue(ParameterPValueProperty); }
            set { SetValue(ParameterPValueProperty, value); }
        }
        public static readonly DependencyProperty ParameterPValueProperty =
            DependencyProperty.Register("ParameterPValue", typeof(string), typeof(PidInputBox), new PropertyMetadata("", AnyParameterChangedCallback));

        public string ParameterIValue
        {
            get { return (string)GetValue(ParameterIValueProperty); }
            set { SetValue(ParameterIValueProperty, value); }
        }
        public static readonly DependencyProperty ParameterIValueProperty =
            DependencyProperty.Register("ParameterIValue", typeof(string), typeof(PidInputBox), new PropertyMetadata("", AnyParameterChangedCallback));

        public string ParameterDValue
        {
            get { return (string)GetValue(ParameterDValueProperty); }
            set { SetValue(ParameterDValueProperty, value); }
        }
        public static readonly DependencyProperty ParameterDValueProperty =
            DependencyProperty.Register("ParameterDValue", typeof(string), typeof(PidInputBox), new PropertyMetadata("", AnyParameterChangedCallback));

        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // Control dependency properties callbacks space below ↓

        /// <summary>Callback wywoływany przy zmianie dowolnego parametru właściwości</summary>
        public static void AnyParameterChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PidInputBox inputBox = (PidInputBox)sender;
            inputBox.PidChanged?.Invoke(inputBox, inputBox.ControlID);
        }



        // ---------------  -----  ---------------  -----  ---------------  -----  ---------------
        // ObservablePropertys space below ↓

        /*public void AnyParameterChanged()
        {
            PidChanged?.Invoke(this, ControlID);
        }*/

        /*public string TemperatureValue
        {
            get => temperatureField.Text;
            set
            {
                temperatureField.Text = value;
                AnyParameterChanged();
            }
        }

        public string ParameterPValue
        {
            get => pField.Text;
            set
            {
                pField.Text = value;
                AnyParameterChanged();
            }
        }

        public string ParameterIValue
        {
            get => iField.Text;
            set
            {
                iField.Text = value;
                AnyParameterChanged();
            }
        }

        public string ParameterDValue
        {
            get => dField.Text;
            set
            {
                dField.Text = value;
                AnyParameterChanged();
            }
        }*/
    }
}
