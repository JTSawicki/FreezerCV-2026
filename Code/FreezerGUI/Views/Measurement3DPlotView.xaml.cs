using FreezerGUI.ViewModels;
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

namespace FreezerGUI.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Measurement3DPlotView.xaml
    /// </summary>
    public partial class Measurement3DPlotView : UserControl
    {
        public Measurement3DPlotView()
        {
            InitializeComponent();
            this.FontSize = BootStrapper.Settings.GlobalFontSize;
            this.DataContext = new Measurement3DPlotVM();

            this.Loaded += Control_Loaded;
        }

        void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += (sender, args) => ((Measurement3DPlotVM)this.DataContext).Dispose();
        }

        private void ResetCamera_ButtonClick(object sender, RoutedEventArgs e)
        {
            Plot3DControl.ResetCamera();
        }
    }
}
