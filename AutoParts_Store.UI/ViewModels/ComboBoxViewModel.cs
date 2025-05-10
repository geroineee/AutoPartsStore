using AutoParts_Store.UI.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

public class ComboBoxViewModel : ViewModelBase
{
    public ObservableCollection<object> Items { get; } = new();
    private object _selectedItem;
    private bool _isLoading;

    private bool _isNullable;

    public object SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ComboBoxViewModel(
        AutoPartsStoreTables tablesService,
        string referenceTable,
        string displayColumn,
        string idColumn,
        object entity,
        string idPropertyName,
        bool isNullable = false) // Добавлен параметр isNullable
    {
        _isNullable = isNullable; // Сохраняем значение isNullable
        // Запускаем загрузку асинхронно
        InitializeAsync(tablesService, referenceTable, displayColumn, idColumn, entity, idPropertyName)
            .ConfigureAwait(false);
    }

    private async Task InitializeAsync(
        AutoPartsStoreTables tablesService,
        string referenceTable,
        string displayColumn,
        string idColumn,
        object entity,
        string idPropertyName)
    {
        try
        {
            IsLoading = true;
            string referenceTableDisplayName = AutoPartsStoreTables.TableDefinitions
                .First(t => t.DbName == referenceTable).DisplayName;

            await LoadItems(tablesService, referenceTableDisplayName, displayColumn, idColumn, entity, idPropertyName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadItems(
        AutoPartsStoreTables tablesService,
        string referenceTableDisplayName,
        string displayColumn,
        string idColumn,
        object entity,
        string idPropertyName)
    {
        try
        {
            Console.WriteLine($"Начало загрузки данных для {referenceTableDisplayName}");

            var items = await tablesService.GetTableDataAsync(referenceTableDisplayName);
            Console.WriteLine($"Получено {items.Count} элементов");

            var currentId = entity.GetType().GetProperty(idPropertyName)?.GetValue(entity);
            Console.WriteLine($"Текущий ID: {currentId}");

            // Используем Dispatcher или аналогичный механизм для обновления UI
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Items.Clear();
                if (_isNullable) // Добавляем "Не выбрано" только если ForeignKey nullable
                {
                    Items.Add(null); // Добавляем "Не выбрано"
                }

                object? matchedItem = null;
                foreach (var item in items)
                {
                    Items.Add(item);
                    var itemId = item.GetType().GetProperty(idColumn)?.GetValue(item);
                    if (itemId?.Equals(currentId) == true)
                    {
                        matchedItem = item;
                        Console.WriteLine($"Найден соответствующий элемент: {item}");
                    }
                }

                // Проверяем, является ли currentId null
                if (currentId == null && _isNullable) // Проверяем _isNullable
                {
                    SelectedItem = null; // Выбираем "Не выбрано", если currentId равен null
                    Console.WriteLine($"Установлен SelectedItem: null (Не выбрано)");
                }
                else
                {
                    SelectedItem = matchedItem;
                    Console.WriteLine($"Установлен SelectedItem: {SelectedItem != null}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            SelectedItem = null;
        }
    }

    public string GetDisplayText(object item, string displayColumn)
    {
        if (item == null) return "Не выбрано";

        try
        {
            var prop = item.GetType().GetProperty(displayColumn);
            if (prop == null) return item.ToString() ?? "Неизвестное значение";

            return prop.GetValue(item)?.ToString() ?? "Пустое значение";
        }
        catch
        {
            return item.ToString() ?? "Ошибка отображения";
        }
    }
}