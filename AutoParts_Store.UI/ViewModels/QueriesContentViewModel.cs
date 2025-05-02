using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class QueriesContentViewModel : ViewModelBase
    {
        private string _header = "Запросы к БД";
        private ObservableCollection<object> _data = [];
        private bool _isLoading;

        public string Header
        {
            get => _header;
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }

        public ObservableCollection<object> Data
        {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public QueriesContentViewModel()
        {
            //LoadData().ConfigureAwait(false);
        }

        public async Task LoadData(int q = 0)
        {
            IsLoading = true;
            try
            {
                List<Object>? query;

                if (q == 1)
                    query = await _queriesService.GetSupplierProvidesProductAsync(1, 1);
                else
                    query = await _queriesService.GetSupplierProvidesProductWithValueAsync(2, 1, 1,
                new DateTime(2005, 1, 1), new DateTime(2030, 1, 1));

                Data = new ObservableCollection<object>(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
