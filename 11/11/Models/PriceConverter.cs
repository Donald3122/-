using System;
using Windows.UI.Xaml.Data;

namespace _11.Models
{
    // Конвертер для отображения цены в формате валюты
    public class PriceConverter : IValueConverter
    {
        // Преобразует десятичное значение цены в строку с форматом валюты
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal price)
            {
                return $"Цена: {price:C}";
            }
            return "Цена не указана";
        }

        // Не реализовано, так как конвертер не поддерживает обратное преобразование
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
