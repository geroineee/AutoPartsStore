using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AutoParts_Store.UI.ViewModels
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DateOnly dateOnly => new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue)),
                DateTime dateTime => new DateTimeOffset(dateTime),
                DateTimeOffset dto => dto,
                null => null,
                _ => throw new ArgumentException($"Неподдерживаемый тип: {value.GetType()}")
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            return value switch
            {
                DateTimeOffset dto => targetType switch
                {
                    Type t when t == typeof(DateOnly) => DateOnly.FromDateTime(dto.DateTime),
                    Type t when t == typeof(DateOnly?) => (DateOnly?)DateOnly.FromDateTime(dto.DateTime),
                    Type t when t == typeof(DateTime) => dto.DateTime,
                    Type t when t == typeof(DateTime?) => (DateTime?)dto.DateTime,
                    Type t when t == typeof(DateTimeOffset) => dto,
                    Type t when t == typeof(DateTimeOffset?) => (DateTimeOffset?)dto,
                    _ => throw new ArgumentException($"Неподдерживаемый целевой тип: {targetType}")
                },
                _ => throw new ArgumentException($"Неподдерживаемый тип: {value.GetType()}")
            };
        }
    }
}
