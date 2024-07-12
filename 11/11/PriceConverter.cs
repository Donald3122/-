using System;
using Windows.UI.Xaml.Data;

namespace _11.Models
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal price)
            {
                return price.ToString("C");
            }
            return value; // Возвращаем значение как есть, если не удалось сконвертировать
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
