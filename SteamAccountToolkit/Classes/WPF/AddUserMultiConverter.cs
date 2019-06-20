using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SteamAccountToolkit.Classes.WPF
{
    public class AddUserMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is PasswordBox userPass))
                return null;
            if (!(values[1] is PasswordBox authSecret))
                return null;

            return new[]
            {
                userPass,
                authSecret
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
