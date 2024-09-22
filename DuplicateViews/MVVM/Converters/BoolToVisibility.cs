using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DuplicateViews.MVVM.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cast the input value to a bool
            bool isVisible = (bool)value;

            // Return Visibility.Visible if true, otherwise Visibility.Collapsed
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert Visibility to boolean
            Visibility visibility = (Visibility)value;

            // Return true if Visibility is Visible, otherwise false
            return visibility == Visibility.Visible;
        }
    }
}
