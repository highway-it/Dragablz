using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dragablz.Converters
{
    public class BooleanAndToVisibilityConverter : IMultiValueConverter
    {
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            return values == null
                ? Visibility.Collapsed
                : (object) ( values.Select ( GetBool ).All ( b => b )
                ? Visibility.Visible
                : Visibility.Collapsed );
        }

        public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            return null;
        }

        private static bool GetBool ( object value )
        {
            return value is bool boolean && boolean;
        }
    }
}