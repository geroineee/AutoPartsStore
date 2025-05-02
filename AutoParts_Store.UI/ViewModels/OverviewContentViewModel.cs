using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoPartsStore.Data;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AutoParts_Store.UI.ViewModels
{
    public class OverviewContentViewModel : ViewModelBase
    {
        DataGrid _dataGrid;
        private string _currentTable;
        private bool _isLoading;
        private ObservableCollection<object> _tableData;

        public List<string> TableDisplayNames => _tablesService.AvailableTables.Keys.ToList();
        
        public ObservableCollection<object> TableData
        {
            get => _tableData;
            set => this.RaiseAndSetIfChanged(ref _tableData, value);
        }

        public string CurrentTable
        {
            get => _currentTable;
            set => this.RaiseAndSetIfChanged(ref _currentTable, value);
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

        public async Task LoadTableDataAsync()
        {
            if (string.IsNullOrEmpty(CurrentTable)) return;

            IsLoading = true;
            try
            {
                var tableName = _tablesService.AvailableTables[CurrentTable];
                var data = await _tablesService.GetTableDataAsync(tableName);
                TableData = new ObservableCollection<object>(data);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
