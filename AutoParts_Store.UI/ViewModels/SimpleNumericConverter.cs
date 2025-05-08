using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class SimpleNumericConverter : IValueConverter
    {
        private readonly Type _targetType;

        public SimpleNumericConverter(Type targetType)
        {
            _targetType = targetType;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование значения в строку
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var underlyingType = Nullable.GetUnderlyingType(_targetType) ?? _targetType;
            var isNullable = Nullable.GetUnderlyingType(_targetType) != null;

            // Обработка null или пустой строки
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return isNullable ? null : GetDefaultValue(underlyingType);
            }

            try
            {
                // Пытаемся преобразовать введенный текст
                var result = System.Convert.ChangeType(value, underlyingType, culture);
                return isNullable && result.Equals(GetDefaultValue(underlyingType)) ? null : result;
            }
            catch
            {
                return isNullable ? null : GetDefaultValue(underlyingType);
            }
        }

        private object GetDefaultValue(Type type)
        {
            return type switch
            {
                Type t when t == typeof(int) => 0,
                Type t when t == typeof(decimal) => 0m,
                Type t when t == typeof(float) => 0f,
                _ => 0
            };
        }
    }
}
