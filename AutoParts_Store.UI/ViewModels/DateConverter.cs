using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AutoParts_Store.UI.ViewModels
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        private static readonly DateTimeOffset _minValidDateTimeOffset;

        static DateTimeToDateTimeOffsetConverter()
        {
            try
            {
                // Безопасная инициализация минимальной даты
                _minValidDateTimeOffset = new DateTimeOffset(new DateTime(1, 1, 1), TimeSpan.Zero);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Если не поддерживается 01.01.0001, используем минимально возможную
                _minValidDateTimeOffset = DateTimeOffset.MinValue;
            }
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                return value switch
                {
                    DateOnly dateOnly => dateOnly == DateOnly.MinValue
                        ? _minValidDateTimeOffset
                        : new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue)),

                    DateTime dateTime => dateTime == DateTime.MinValue
                        ? _minValidDateTimeOffset
                        : new DateTimeOffset(dateTime),

                    DateTimeOffset dto => dto,
                    null => null,
                    _ => throw new ArgumentException($"Неподдерживаемый тип: {value?.GetType()}")
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка конвертации даты: {ex.Message}");
                return _minValidDateTimeOffset;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                if (value is DateTimeOffset dto)
                {
                    if (dto == _minValidDateTimeOffset)
                    {
                        return targetType switch
                        {
                            Type t when t == typeof(DateOnly) => DateOnly.MinValue,
                            Type t when t == typeof(DateOnly?) => (DateOnly?)DateOnly.MinValue,
                            Type t when t == typeof(DateTime) => DateTime.MinValue,
                            Type t when t == typeof(DateTime?) => (DateTime?)DateTime.MinValue,
                            Type t when t == typeof(DateTimeOffset) => _minValidDateTimeOffset,
                            Type t when t == typeof(DateTimeOffset?) => (DateTimeOffset?)_minValidDateTimeOffset,
                            _ => throw new ArgumentException($"Неподдерживаемый целевой тип: {targetType}")
                        };
                    }

                    return targetType switch
                    {
                        Type t when t == typeof(DateOnly) => DateOnly.FromDateTime(dto.DateTime),
                        Type t when t == typeof(DateOnly?) => (DateOnly?)DateOnly.FromDateTime(dto.DateTime),
                        Type t when t == typeof(DateTime) => dto.DateTime,
                        Type t when t == typeof(DateTime?) => (DateTime?)dto.DateTime,
                        Type t when t == typeof(DateTimeOffset) => dto,
                        Type t when t == typeof(DateTimeOffset?) => (DateTimeOffset?)dto,
                        _ => throw new ArgumentException($"Неподдерживаемый целевой тип: {targetType}")
                    };
                }

                throw new ArgumentException($"Неподдерживаемый тип: {value.GetType()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обратной конвертации: {ex.Message}");
                return null;
            }
        }
    }
}
