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

namespace LabControlsWPF.Plot2D
{
    /// <summary>
    /// Logika interakcji dla klasy MultiSeriesPlot.xaml
    /// </summary>
    public partial class MultiSeriesPlot : UserControl
    {


        public MultiSeriesPlotModel MultiSeriesPlotModel
        {
            get { return (MultiSeriesPlotModel)GetValue(MultiSeriesPlotModelProperty); }
            set { SetValue(MultiSeriesPlotModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MultiSeriesPlotModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MultiSeriesPlotModelProperty =
            DependencyProperty.Register(
                "MultiSeriesPlotModel",
                typeof(MultiSeriesPlotModel),
                typeof(MultiSeriesPlot),
                new UIPropertyMetadata(MultiSeriesPlotModelPropertyCallback)
                );



        public MultiSeriesPlot()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Funkcja odświeża widok wykresu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CenterOnPlotClick(object sender, RoutedEventArgs e)
        {
            Plot.ResetAllAxes();
        }

        /// <summary>
        /// Potrzebne aby działały wiązania danych.
        /// Nie wiem czemu XD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void MultiSeriesPlotModelPropertyCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((MultiSeriesPlot)sender).Plot.Model = ((MultiSeriesPlotModel)e.NewValue).OxyPlotModel;
        }
    }
}
