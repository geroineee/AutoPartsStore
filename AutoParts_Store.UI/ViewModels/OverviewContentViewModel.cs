using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using Avalonia.Notification;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace AutoParts_Store.UI.ViewModels
{
    public class OverviewContentViewModel : ViewModelBase
    {
        private DataGrid _dataGrid;
        private string _currentTable;
        private bool _isLoading;
        private ObservableCollection<object> _tableData;

        private ObservableCollection<string> _dataGridColumnsList = [];
        private string _searchColumn;
        private string _searchText;

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
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public List<string> TableDisplayNames => _tablesService.AvailableTables.Keys.ToList();

        public ObservableCollection<object> TableData
        {
            get => _tableData;
            set => this.RaiseAndSetIfChanged(ref _tableData, value);
        }

        public ObservableCollection<string> DataGridColumnsList
        {
            get => _dataGridColumnsList;
            set => this.RaiseAndSetIfChanged(ref _dataGridColumnsList, value);
        }

        public string CurrentTable
        {
            get => _currentTable;
            set
            {
                if (_currentTable != value)
                {
                    this.RaiseAndSetIfChanged(ref _currentTable, value);
                    UpdateDataGridColumns();
                    _ = LoadTableDataAsync();
                }
            }
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

            MainWindowViewModel.Instance.ChangeContent(typeof(EditContentViewModel));

            if (MainWindowViewModel.Instance.ContentViewModel is EditContentViewModel editVM)
            {
                editVM.TableName = CurrentTable;
                editVM.CurrentItem = itemCopy;
                editVM.OriginalEntity = originalEntity;
                editVM.CreateEditControls();
                CurrentTable = null;
            }
        }

        private object? FindOriginalEntity(object selectedItem, string tableDisplayName)
        {
            try
            {
                var tableDef = AutoPartsStoreTables.TableDefinitions
                    .FirstOrDefault(t => t.DisplayName == tableDisplayName);

                if (tableDef == null) return null;

                var idColumn = tableDef.Columns.FirstOrDefault(c => c.IsId);
                if (idColumn == null)
                {
                    idColumn = tableDef.Columns.FirstOrDefault(c =>
                        c.PropertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                }

                if (idColumn == null) return null;
                var idProperty = selectedItem.GetType().GetProperty(idColumn.PropertyName);
                if (idProperty == null) return null;

                var idValue = idProperty.GetValue(selectedItem);
                if (idValue == null) return null;

                return _tablesService.GetItemByIdAsync(tableDef.DbName, idValue).Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task ExecuteSearch()
        {
            if (string.IsNullOrEmpty(SearchColumn) && string.IsNullOrEmpty(SearchText))
            {
                _currentNotification = CreateNotification("Предупреждение", "Необходимо указать поле и текст поиска",
                    NotificationManager,
                    _currentNotification);
            }

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
                var column = new DataGridTextColumn
                {
                    Header = columnInfo.DisplayName,
                    Binding = new Binding(columnInfo.PropertyName),
                };

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
            catch (DbUpdateException ex)
            {
                _currentNotification = CreateNotification("Ошибка", $"Ошибка удаления записи: запись используется в других таблицах", NotificationManager, _currentNotification);
            }
            catch (Exception ex)
            {
                _currentNotification = CreateNotification("Ошибка", $"{ex.Message}, {ex.GetType()}", NotificationManager, _currentNotification);
            }
            finally
            {
                IsLoading = false;
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
            var clone = Activator.CreateInstance(source.GetType());

            foreach (var property in source.GetType().GetProperties())
            {
                if (property.PropertyType.IsValueType ||
                    property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(source);
                    property.SetValue(clone, value);
                }
            }

            return clone;
        }
    }
}