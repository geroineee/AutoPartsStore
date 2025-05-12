using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoPartsStore.Data;

namespace AutoParts_Store.UI.ViewModels
{
    public class ParameterViewModel : ViewModelBase
    {
        private string _displayName;
        private object _selectedValue;
        private bool _isLoading;
        private bool _isVisible = true;

        public ParameterViewModel() { }

        public ParameterViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public virtual object SelectedValue
        {
            get => _selectedValue;
            set => this.RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
    }

    public class ComboBoxParameterViewModel : ParameterViewModel
    {
        public ObservableCollection<object> Items { get; } = new();
        private string _displayMember;
        private string _valueMember;
        private object _selectedValue;
        private AutoPartsStoreTables _tablesService;
        private string _tableName;

        public ComboBoxParameterViewModel(string displayName, AutoPartsStoreTables tablesService, string tableName, string displayMember, string valueMember) : base(displayName)
        {
            _displayMember = displayMember;
            _valueMember = valueMember;
            _tablesService = tablesService;
            _tableName = tableName;
            LoadItems().ConfigureAwait(false);
        }
        public string DisplayMember { get => _displayMember; set => _displayMember = value; }
        public string ValueMember { get => _valueMember; set => _valueMember = value; }

        public override object SelectedValue
        {
            get => _selectedValue;
            set => this.RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        private async Task LoadItems()
        {
            IsLoading = true;
            try
            {
                var items = await _tablesService.GetTableDataAsync(_tableName);
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Items.Clear();
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
