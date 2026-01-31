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
using OxyPlot;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Serilog;

namespace LabControlsWPF.Plot2D
{
    /// <summary>
    /// Logika interakcji dla klasy RealTimePlot.xaml
    /// </summary>
    public partial class RealTimePlot : UserControl
    {
        public RealTimePlotModel RealTimePlotModel
        {
            get { return (RealTimePlotModel)GetValue(RealTimePlotModelProperty); }
            set { SetValue(RealTimePlotModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RealTimePlotModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RealTimePlotModelProperty =
            DependencyProperty.Register(
                "RealTimePlotModel",
                typeof(RealTimePlotModel),
                typeof(RealTimePlot),
                new UIPropertyMetadata(RealTimePlotModelPropertyCallback)
                );

        public RealTimePlot()
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
        /// Funkcja przełącznika blokady wykresu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PausePlayToggleClick(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton)
                return;

            if ( ((ToggleButton)sender).IsChecked == true)
            {
                RealTimePlotModel.PausePlot = true;
            }
            else
            {
                RealTimePlotModel.PausePlot = false;
                RealTimePlotModel.FlushBuffers();
            }
        }

        /// <summary>
        /// Potrzebne aby działały wiązania danych.
        /// Nie wiem czemu XD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RealTimePlotModelPropertyCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((RealTimePlot)sender).Plot.Model = ((RealTimePlotModel)e.NewValue).OxyPlotModel;
        }
    }
}
