using AutoParts_Store.UI.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using ReactiveUI;
using System.Linq;

public class ComboBoxViewModel : ViewModelBase
{
    public ObservableCollection<object> Items { get; } = new();
    private object _selectedItem;

    public object SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ComboBoxViewModel(
        AutoPartsStoreTables tablesService,
        string referenceTable,
        string displayColumn,
        string idColumn,
        object entity,
        string idPropertyName)
    {
        string referenceTableDisplayName = AutoPartsStoreTables.TableDefinitions.First(t => t.DbName == referenceTable).DisplayName;

        _ = LoadItems(tablesService, referenceTableDisplayName, displayColumn, idColumn, entity, idPropertyName);
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
            var items = await tablesService.GetTableDataAsync(referenceTableDisplayName);
            var currentId = entity.GetType().GetProperty(idPropertyName)?.GetValue(entity);

            Items.Clear();

            // Временная переменная для хранения найденного элемента
            object matchedItem = null;

            foreach (var item in items)
            {
                Items.Add(item);
                var itemId = item.GetType().GetProperty(idColumn)?.GetValue(item);
                if (itemId?.Equals(currentId) == true)
                {
                    matchedItem = item;
                }
            }

            // Устанавливаем SelectedItem после полной загрузки
            SelectedItem = matchedItem;
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