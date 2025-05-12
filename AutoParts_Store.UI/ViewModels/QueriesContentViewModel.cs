using AutoParts_Store.UI.Services;
using AutoPartsStore.Data.Context;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class QueriesContentViewModel : ViewModelBase
    {
        private ObservableCollection<object> _data = [];
        private bool _isLoading;
        private QueryDefinition? _selectedQuery; // Выбранный запрос
        private QueryVariation? _selectedVariation; // Выбранная вариация

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

        public ObservableCollection<QueryDefinition> QueryDefinitions { get; set; } = new ObservableCollection<QueryDefinition>(); // Список запросов

        public QueryDefinition? SelectedQuery
        {
            get => _selectedQuery;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedQuery, value);
                SelectedVariation = null; // Сбрасываем выбранную вариацию при смене запроса
                this.RaisePropertyChanged(nameof(Variations)); // Обновляем список вариаций
            }
        }

        public QueryVariation? SelectedVariation
        {
            get => _selectedVariation;
            set => this.RaiseAndSetIfChanged(ref _selectedVariation, value);
        }
        // Вариации для выбранного запроса
        public IEnumerable<QueryVariation> Variations => SelectedQuery?.Variations ?? [];

        //Список контролов для ввода параметров
        public ObservableCollection<Control> InputControls { get; set; } = new ObservableCollection<Control>(); 

        public QueriesContentViewModel(Func<AutopartsStoreContext> dbContextFactory) : base(dbContextFactory)
        {
            LoadQueryDefinitions();
        }

        private void LoadQueryDefinitions()
        {
            // Здесь заполняем QueryDefinitions списком запросов и их вариаций
            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "Поставщики по категории и товару",
                Description = "Получить перечень поставщиков определенной категории, поставляющих указанный вид товара.",
                Variations = new List<QueryVariation>
            {
                new QueryVariation
                {
                    DisplayName = "С указанным товаром",
                    Description = "Запрос поставщиков с фильтром по товару.",
                    ExecutionFunction = async (parameters) =>
                    {
                        int productId = (int)parameters[0];
                        int supplierCategory = (int)parameters[1];
                        return (List<object>)await _queriesService.GetSupplierProvidesProductAsync(productId, supplierCategory);
                    },
                    Parameters = new List<QueryParameter>
                    {
                         new QueryParameter { DisplayName = "ID товара", PropertyName = "ProductId", ParameterType = typeof(int), InputType = QueryParameterType.TextBox },
                         new QueryParameter { DisplayName = "Категория поставщика", PropertyName = "SupplierCategory", ParameterType = typeof(int), InputType = QueryParameterType.TextBox }
                    }
                },
                new QueryVariation
                {
                    DisplayName = "С указанным объемом за период",
                    Description = "Запрос поставщиков с фильтром по объему поставки за период.",
                    ExecutionFunction = async (parameters) =>
                    {
                        int productId = (int)parameters[0];
                        int supplierCategory = (int)parameters[1];
                        int value = (int)parameters[2];
                        DateTime startDate = (DateTime)parameters[3];
                        DateTime endDate = (DateTime)parameters[4];

                        return (List<object>)await _queriesService.GetSupplierProvidesProductWithValueAsync(productId, supplierCategory, value, startDate, endDate);
                    },
                    Parameters = new List<QueryParameter>
                    {
                        new QueryParameter { DisplayName = "ID товара", PropertyName = "ProductId", ParameterType = typeof(int), InputType = QueryParameterType.TextBox },
                        new QueryParameter { DisplayName = "Категория поставщика", PropertyName = "SupplierCategory", ParameterType = typeof(int), InputType = QueryParameterType.TextBox },
                        new QueryParameter { DisplayName = "Объем поставки", PropertyName = "Value", ParameterType = typeof(int), InputType = QueryParameterType.TextBox },
                        new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                        new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                    }
                }
            }
            });
            // Добавьте остальные запросы аналогично
        }

        public async Task ExecuteQuery()
        {
            if (SelectedVariation == null) return;

            IsLoading = true;
            try
            {
                // 1. Создать массив для параметров
                object[] parameters = new object[SelectedVariation.Parameters.Count];

                // 2. Заполнить массив параметров данными из UI (предположим, что у вас есть TextBox'ы для ввода)
                for (int i = 0; i < SelectedVariation.Parameters.Count; i++)
                {
                    //Тут нужно будет брать данные из UI, пока что заглушка
                    if (SelectedVariation.Parameters[i].ParameterType == typeof(int))
                    {
                        parameters[i] = 1;
                    }
                    else if (SelectedVariation.Parameters[i].ParameterType == typeof(DateTime))
                    {
                        parameters[i] = DateTime.Now;
                    }
                }

                //Выполнить запрос
                Data = new ObservableCollection<object>(await SelectedVariation.ExecutionFunction(parameters));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateInputControls()
        {
            InputControls.Clear();

            if (SelectedVariation != null)
            {
                foreach (var parameter in SelectedVariation.Parameters)
                {
                    Control inputControl = CreateInputControl(parameter);
                    InputControls.Add(inputControl);
                }
            }
        }

        private Control CreateInputControl(QueryParameter parameter)
        {
            Control control = parameter.InputType switch
            {
                QueryParameterType.TextBox => new TextBox { Watermark = parameter.DisplayName },
                QueryParameterType.ComboBox => new ComboBox { PlaceholderText = parameter.DisplayName },
                QueryParameterType.DatePicker => new DatePicker(),
                _ => new TextBox { Watermark = "Неизвестный тип" }
            };

            return control;
        }
    }

}
