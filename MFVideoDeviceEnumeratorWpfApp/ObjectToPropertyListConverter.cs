using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MFVideoDeviceEnumeratorWpfApp
{
    public class ObjectToPropertyListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var properties = value?.GetType().GetProperties();

            return properties?.Select(p => $"{p.Name}: {p.GetValue(value)}").ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}