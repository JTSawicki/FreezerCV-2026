using System;
using System.Windows.Data;
using System.Windows;

namespace FreezerGUI.DataConverters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class SoftBooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException($"The target must be Visibility. RealType: {targetType.Name}");

            if ((bool)value)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException($"The target must be Boolean. RealType: {targetType.Name}");

            if ((Visibility)value == Visibility.Visible)
                return true;
            return false;
        }
    }
}
