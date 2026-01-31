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
    /// Logika interakcji dla klasy SystemStateView.xaml
    /// </summary>
    public partial class SystemStateView : UserControl
    {
        public SystemStateView()
        {
            InitializeComponent();
            this.FontSize = BootStrapper.Settings.GlobalFontSize;
            this.DataContext = new SystemStateVM();

            this.Loaded += Control_Loaded;
        }

        void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += (sender, args) => ((SystemStateVM)this.DataContext).Dispose();
        }
    }
}
