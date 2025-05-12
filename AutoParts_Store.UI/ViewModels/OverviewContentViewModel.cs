using AutoParts_Store.UI.Services;
using AutoPartsStore.Data.Context;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Notification;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class OverviewContentViewModel : ViewModelBase
    {
        private DataGrid _dataGrid;
        private string? _currentTable;
        private bool _isLoading;
        private ObservableCollection<object> _tableData;

        private ObservableCollection<string> _dataGridColumnsList = [];
        private string? _searchColumn;
        private string? _searchText;

        private INotificationMessage? _currentNotification;
        public INotificationMessageManager NotificationManager { get; } = new NotificationMessageManager();

        public string SearchColumn
        {
            get => _searchColumn;
            set => this.RaiseAndSetIfChanged(ref _searchColumn, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                _ = ExecuteSearch();
            }
        }

        public List<string> TableDisplayNames => _tablesService.AvailableTables.Keys.ToList();

        public ObservableCollection<object> TableData
        {
            get => _tableData;
            set
            {
                this.RaiseAndSetIfChanged(ref _tableData, value);
            }
        }

        public ObservableCollection<string> DataGridColumnsList
        {
            get => _dataGridColumnsList;
            set
            {
                this.RaiseAndSetIfChanged(ref _dataGridColumnsList, value);
            }
        }

        public string CurrentTable
        {
            get => _currentTable;
            set
            {
                if (_currentTable != value)
                {
                    this.RaiseAndSetIfChanged(ref _currentTable, value);
                    _ = LoadTableDataAndColumnsAsync();
                }
            }
        }

        public OverviewContentViewModel(Func<AutopartsStoreContext> dbContextFactoryFunc) : base(dbContextFactoryFunc)
        {
            TableData = new ObservableCollection<object>();
        }

        private async Task LoadTableDataAndColumnsAsync()
        {
            await LoadTableDataAsync();
            UpdateDataGridColumns();

            SearchColumn = DataGridColumnsList[0];
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public OverviewContentViewModel()
        {
            TableData = new ObservableCollection<object>();
        }

        public void SwitchOnEditContent(object selectedItem)
        {
            if (selectedItem == null || string.IsNullOrEmpty(CurrentTable)) return;

            var originalEntity = FindOriginalEntity(selectedItem, CurrentTable);

            var itemCopy = CloneObject(originalEntity!);

            if (originalEntity == null || itemCopy == null)
            {
                _currentNotification = CreateNotification("Ошибка", "Не удалось начать редактирование", NotificationManager, _currentNotification);
                return;
            }

            MainWindowViewModel.Instance?.ChangeContent(typeof(EditContentViewModel));

            if (MainWindowViewModel.Instance?.ContentViewModel is EditContentViewModel editVM)
            {
                editVM.TableName = CurrentTable;
                editVM.AnonymousItem = selectedItem;
                editVM.CurrentItem = itemCopy;
                editVM.OriginalEntity = originalEntity;
                editVM.CreateEditControls();
                CurrentTable = null!;
            }
        }

        private object? FindOriginalEntity(object selectedItem, string tableDisplayName)
        {
            try
            {
                var tableDef = AutoPartsStoreTables.TableDefinitions
                    .FirstOrDefault(t => t.DisplayName == tableDisplayName);

                if (tableDef == null) return null;

                // Получаем все колонки, которые являются частью ключа (Id или составного ключа)
                var keyColumns = tableDef.Columns
                    .Where(c => c.IsId || c.IsCompositeKey)
                    .ToList();

                if (!keyColumns.Any())
                {
                    // Если нет явных ключевых колонок, ищем по свойству, заканчивающемуся на Id
                    keyColumns = tableDef.Columns
                        .Where(c => c.PropertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!keyColumns.Any()) return null;
                }

                // Если ключ составной - собираем все значения
                if (keyColumns.Count > 1 || keyColumns.Any(c => c.IsCompositeKey))
                {
                    var keyValues = new Dictionary<string, object>();
                    foreach (var keyColumn in keyColumns)
                    {
                        var prop = selectedItem.GetType().GetProperty(keyColumn.ForeignKeyProperty);
                        if (prop == null) return null;

                        var value = prop.GetValue(selectedItem);
                        if (value == null) return null;

                        keyValues[keyColumn.ForeignKeyProperty] = value;
                    }
                    using (var db = _dbContextFactoryFunc())
                        return _tablesService.GetCompositeKeyItemAsync(db, tableDef.DbName, keyValues).Result;
                }
                else
                {
                    // Обычный одиночный ключ
                    var idColumn = keyColumns.First();
                    var idProperty = selectedItem.GetType().GetProperty(idColumn.PropertyName);
                    if (idProperty == null) return null;

                    var idValue = idProperty.GetValue(selectedItem);
                    if (idValue == null) return null;

                    using (var db = _dbContextFactoryFunc())
                        return _tablesService.GetItemByIdAsync(db, tableDef.DbName, idValue).Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindOriginalEntity: {ex.Message}");
                return null;
            }
        }

        public async Task ExecuteSearch()
        {
            await SearchInCurrentTable(SearchColumn, SearchText);
        }

        public void AttachDataGrid(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            UpdateDataGridColumns();
        }

        private void UpdateDataGridColumns()
        {
            if (_dataGrid == null || string.IsNullOrEmpty(CurrentTable))
                return;

            _dataGrid.Columns.Clear();
            _dataGridColumnsList.Clear();

            var tableDefinition = AutoPartsStoreTables.TableDefinitions.First(tbl => tbl.DisplayName == CurrentTable);

            foreach (var columnInfo in tableDefinition.Columns.Where(c => c.IsVisible))
            {
                DataGridColumn column;
                var propertyInfo = TableData.FirstOrDefault()?.GetType().GetProperty(columnInfo.PropertyName);

                if (propertyInfo != null && (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(decimal?)))
                {
                    // Create a SimpleNumericConverter with the "N2" format
                    var converter = new SimpleNumericConverter(propertyInfo.PropertyType, "N2");

                    column = new DataGridTextColumn
                    {
                        Header = columnInfo.DisplayName,
                        Binding = new Binding(columnInfo.PropertyName) { Converter = converter },
                    };
                }
                else if (propertyInfo != null && (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?)))
                {
                    column = new DataGridTemplateColumn
                    {
                        Header = columnInfo.DisplayName,
                        CellTemplate = new FuncDataTemplate<object>((item, _) =>
                        {
                            var checkBox = new CheckBox
                            {
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                IsEnabled = false
                            };
                            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(columnInfo.PropertyName) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                            return checkBox;
                        })
                    };
                }
                else
                {
                    column = new DataGridTextColumn
                    {
                        Header = columnInfo.DisplayName,
                        Binding = new Binding(columnInfo.PropertyName),
                    };
                }

                _dataGridColumnsList.Add(columnInfo.DisplayName);
                _dataGrid.Columns.Add(column);
            }

            _dataGrid.ItemsSource = TableData;
        }

        public async Task DeleteTableDataItem(object selectedItem)
        {
            if (selectedItem == null || string.IsNullOrEmpty(CurrentTable))
            {
                _currentNotification = CreateNotification("Предупреждение", "Необходимо выбрать запись", NotificationManager, _currentNotification);
                return;
            }

            if (!await ShowConfirmationDialog("Внимание!", $"Вы уверены что хотите удалить\n" +
                $"элемент таблицы {CurrentTable}?"))
                return;

            try
            {
                IsLoading = true;

                var tableDefinition = AutoPartsStoreTables.TableDefinitions
                    .FirstOrDefault(t => t.DisplayName == CurrentTable);

                if (tableDefinition == null)
                    throw new ArgumentException("Table definition not found");

                await _tablesService.DeleteTableItemAsync(tableDefinition.DisplayName, selectedItem);

                await LoadTableDataAsync();
            }
            catch (Exception ex)
            {
                _currentNotification = CreateNotification("Ошибка", $"Ошибка удаления записи: запись используется в других таблицах", NotificationManager, _currentNotification);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void CreateNewItem()
        {
            if (CurrentTable == null)
            {
                _currentNotification = CreateNotification("Напоминание", "Выберите таблицу", NotificationManager, _currentNotification);
                return;
            }


            var tableDefinition = AutoPartsStoreTables.TableDefinitions.FirstOrDefault(t => t.DisplayName == CurrentTable);
            if (tableDefinition == null)
            {
                _currentNotification = CreateNotification("Ошибка", "Не найдено определение таблицы", NotificationManager, _currentNotification);
                return;
            }

            var entityType = tableDefinition.TableType;

            if (entityType == null)
            {
                _currentNotification = CreateNotification("Ошибка",
                    $"Не удалось определить тип таблицы",
                    NotificationManager,
                    _currentNotification);
                return;
            }

            var newItem = Activator.CreateInstance(entityType);

            MainWindowViewModel.Instance?.ChangeContent(typeof(EditContentViewModel));
            if (MainWindowViewModel.Instance?.ContentViewModel is EditContentViewModel editVM)
            {
                editVM.TableName = CurrentTable;
                editVM.CurrentItem = newItem!;
                editVM.OriginalEntity = null;
                editVM.CreateEditControls();
            }
        }


        public async Task LoadTableDataAsync()
        {
            if (string.IsNullOrEmpty(CurrentTable)) return;

            IsLoading = true;
            try
            {
                var data = await _tablesService.GetTableDataAsync(CurrentTable);
                TableData = new ObservableCollection<object>(data);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchInCurrentTable(string columnName, string searchValue)
        {
            if (string.IsNullOrEmpty(CurrentTable))
            {
                throw new Exception("Не выбрана таблица базы данных");
            }

            IsLoading = true;
            try
            {
                var data = await _tablesService.SearchInTableAsync(CurrentTable, columnName, searchValue);
                TableData = new ObservableCollection<object>(data);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected static object? CloneObject(object source)
        {
            if (source == null)
                return null;

            var type = source.GetType();
            var clone = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties())
            {
                if (!property.CanWrite)
                    continue;

                var value = property.GetValue(source);
                property.SetValue(clone, value);
            }

            return clone;
        }
    }
}