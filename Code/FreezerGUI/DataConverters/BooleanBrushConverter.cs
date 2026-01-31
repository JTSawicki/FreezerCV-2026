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
    [ValueConversion(typeof(bool?), typeof(Brush))]
    class BooleanBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException($"The target must be Brush. RealType: {targetType.Name}");

            if (value == null)
                return new SolidColorBrush(Colors.Black);
            if ((bool)value)
                return new SolidColorBrush(Colors.Green);
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool?) || targetType != typeof(bool))
                throw new InvalidOperationException($"The target must be Nullable Boolean. RealType: {targetType.Name}");

            if (((SolidColorBrush)value).Color.Equals(Colors.Black))
                return null;
            if (((SolidColorBrush)value).Color.Equals(Colors.Green))
                return true;
            return false;
        }
    }
}
