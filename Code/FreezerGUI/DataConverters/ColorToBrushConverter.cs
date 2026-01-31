using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace FreezerGUI.DataConverters
{
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException($"The target must be Brush. RealType: {targetType.Name}");

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Color))
                throw new InvalidOperationException($"The target must be Color. RealType: {targetType.Name}");

            return ((SolidColorBrush)value).Color;
        }
    }
}
