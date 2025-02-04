using System;
using System.Globalization;
using System.Windows.Data;

namespace XSPSX
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualWidth && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
            {
                return actualWidth * percentage;
            }
            return 0; // Default to 0 if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
}
