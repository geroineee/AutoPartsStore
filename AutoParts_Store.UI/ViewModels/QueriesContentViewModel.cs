using AutoPartsStore.Data.Context;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Linq;
using AutoParts_Store.UI.Services;

namespace AutoParts_Store.UI.ViewModels
{
    public class QueriesContentViewModel : ViewModelBase
    {
        private ObservableCollection<object> _data = [];
        private bool _isLoading;
        private ObservableCollection<Control> _queriesControls = [];
        private QueryVariation? _selectedQueryVariation;
        private ObservableCollection<QueryDefinition> _queryDefinitions = [];
        private QueryDefinition? _selectedQueryDefinition;
        private Dictionary<string, object> _parameterValues = new Dictionary<string, object>();

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

        public ObservableCollection<Control> QueriesControls
        {
            get => _queriesControls;
            set => this.RaiseAndSetIfChanged(ref _queriesControls, value);
        }

        public ObservableCollection<QueryDefinition> QueryDefinitions
        {
            get => _queryDefinitions;
            set => this.RaiseAndSetIfChanged(ref _queryDefinitions, value);
        }

        public QueryDefinition? SelectedQueryDefinition
        {
            get => _selectedQueryDefinition;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedQueryDefinition, value);
                if (value?.Variations != null && value.Variations.Any())
                {
                    SelectedQueryVariation = value.Variations.First();
                }
            }
        }

        public QueryVariation? SelectedQueryVariation
        {
            get => _selectedQueryVariation;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedQueryVariation, value);
                CreateQueryControls(value);
            }
        }

        public QueriesContentViewModel(Func<AutopartsStoreContext> dbContextFactory) : base(dbContextFactory)
        {
            QueryDefinitions = new ObservableCollection<QueryDefinition>();
            LoadQueryDefinitions();
            if (QueryDefinitions.Any())
            {
                SelectedQueryDefinition = QueryDefinitions.First();
            }
        }

        private void CreateQueryControls(QueryVariation? queryVariation)
        {
            QueriesControls.Clear();
            _parameterValues.Clear();

            if (queryVariation == null) return;

            foreach (var parameter in queryVariation.Parameters)
            {
                Control control = parameter.InputType switch
                {
                    QueryParameterType.TextBox => CreateTextBox(parameter),
                    QueryParameterType.DatePicker => CreateDatePicker(parameter),
                    QueryParameterType.ComboBox => CreateComboBox(parameter),
                    _ => throw new ArgumentOutOfRangeException($"Unsupported input type: {parameter.InputType}")
                };
                QueriesControls.Add(control);
            }
        }

        private Control CreateTextBox(QueryParameter parameter)
        {
            var textBox = new TextBox { Watermark = parameter.DisplayName, Width = 200 };
            textBox.LostFocus += (sender, e) =>
            {
                _parameterValues[parameter.PropertyName] = textBox.Text;
            };

            var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            panel.Children.Add(textBox);
            panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            textBox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            return panel;
        }

        private Control CreateDatePicker(QueryParameter parameter)
        {
            var datePicker = new DatePicker { Width = 400 };
            datePicker.SelectedDateChanged += (sender, e) =>
            {
                _parameterValues[parameter.PropertyName] = datePicker.SelectedDate;
            };
            var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            panel.Children.Add(datePicker);
            panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            datePicker.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            return panel;
        }

        private Control CreateComboBox(QueryParameter parameter)
        {
            var comboBox = new ComboBox { Width = 200 };
            var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            panel.Children.Add(comboBox);
            panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            comboBox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            return comboBox;
        }

        public async Task ExecuteQuery()
        {
            if (SelectedQueryVariation == null) return;

            try
            {
                var parameters = SelectedQueryVariation.Parameters.Select(p => _parameterValues.ContainsKey(p.PropertyName) ? _parameterValues[p.PropertyName] : null).ToArray();
                Data = new ObservableCollection<object>((List<object>)await SelectedQueryVariation.ExecutionFunction(parameters));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
            }
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
                            int productId = parameters[0] != null ? int.Parse(parameters[0].ToString()) : 0;
                            int supplierCategory = parameters[1] != null ? int.Parse(parameters[1].ToString()) : 0;
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
                            int productId = parameters[0] != null ? int.Parse(parameters[0].ToString()) : 0;
                            int supplierCategory = parameters[1] != null ? int.Parse(parameters[1].ToString()) : 0;
                            int value = parameters[2] != null ? int.Parse(parameters[2].ToString()) : 0;
                            DateTime startDate = parameters[3] != null ? (DateTime)parameters[3] : DateTime.MinValue;
                            DateTime endDate = parameters[4] != null ? (DateTime)parameters[4] : DateTime.MaxValue;

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
