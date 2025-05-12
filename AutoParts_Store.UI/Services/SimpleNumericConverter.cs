using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AutoParts_Store.UI.ViewModels
{
    public class SimpleNumericConverter : IValueConverter
    {
        private readonly Type _targetType;
        private readonly string _formatString;

        public SimpleNumericConverter(Type targetType, string formatString = null)
        {
            _targetType = targetType;
            _formatString = formatString ?? "N2"; // Default to "N2" if no format string is provided
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование значения в строку с заданным форматом
            if (value is decimal || value is decimal?)
            {
                return ((decimal)value).ToString(_formatString, culture);
            }
            else if (value is float || value is float?)
            {
                return ((float)value).ToString(_formatString, culture);
            }
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