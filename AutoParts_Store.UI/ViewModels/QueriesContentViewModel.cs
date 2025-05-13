using AutoPartsStore.Data.Context;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Linq;
using AutoParts_Store.UI.Services;
using Avalonia.Controls.Templates;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Notification;

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

        private string _header = "Название запроса";
        private string _description = "Описание запроса";

        private ObservableCollection<ParameterViewModel> _parameters = [];

        private INotificationMessage? _currentNotification;
        public INotificationMessageManager NotificationManager { get; } = new NotificationMessageManager();

        public string Header
        {
            get => _header;
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public ObservableCollection<object> Data
        {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }
        public ObservableCollection<ParameterViewModel> Parameters
        {
            get => _parameters;
            set => this.RaiseAndSetIfChanged(ref _parameters, value);
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
                Header = SelectedQueryDefinition.DisplayName;
                Description = SelectedQueryDefinition.Description;

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
        }

        private async void CreateQueryControls(QueryVariation? queryVariation)
        {
            QueriesControls.Clear();
            _parameterValues.Clear();

            if (!MainWindowViewModel.Instance.IsAuthenticated) return;

            if (queryVariation == null) return;

            List<Task<Control>> controlCreationTasks = new List<Task<Control>>();

            foreach (var parameter in queryVariation.Parameters)
            {
                Task<Control> controlCreationTask = parameter.InputType switch
                {
                    QueryParameterType.TextBox => Task.FromResult(CreateTextBox(parameter)),
                    QueryParameterType.DatePicker => Task.FromResult(CreateDatePicker(parameter)),
                    QueryParameterType.ComboBox => CreateComboBox(parameter),
                    QueryParameterType.NumericUpDown => Task.FromResult(CreateNumericUpDown(parameter)),
                    _ => throw new ArgumentOutOfRangeException($"Unsupported input type: {parameter.InputType}")
                };
                controlCreationTasks.Add(controlCreationTask);
            }

            var controls = await Task.WhenAll(controlCreationTasks);

            foreach (var control in controls)
            {
                QueriesControls.Add(control);
            }
        }

        private Control CreateNumericUpDown(QueryParameter parameter)
        {
            // Текстовая метка
            var label = new TextBlock
            {
                Text = parameter.DisplayName,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeight.Bold
            };

            // NumericUpDown контрол
            var numericUpDown = new NumericUpDown
            {
                Width = 200,
                Margin = new Thickness(0, 0, 10, 0),
                Minimum = 0,
                Maximum = int.MaxValue,
                FormatString = "0", // Исправленный формат (убрали "D")
                AllowSpin = true,
                ShowButtonSpinner = true
            };

            // Установка корректного типа значения
            if (parameter.ParameterType == typeof(decimal) ||
                parameter.ParameterType == typeof(double) ||
                parameter.ParameterType == typeof(float))
            {
                numericUpDown.FormatString = "0.00"; // Формат для дробных чисел
            }

            // Кнопка очистки
            var clearButton = new Button
            {
                Content = "Очистить",
                Width = 80,
                Command = ReactiveCommand.Create(() =>
                {
                    numericUpDown.Value = null;
                    _parameterValues[parameter.PropertyName] = null;
                })
            };

            numericUpDown.ValueChanged += (sender, e) =>
            {
                try
                {
                    if (numericUpDown.Value.HasValue)
                    {
                        // Конвертируем в нужный тип
                        _parameterValues[parameter.PropertyName] =
                            Convert.ChangeType(numericUpDown.Value.Value, parameter.ParameterType);
                    }
                    else
                    {
                        _parameterValues[parameter.PropertyName] = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка преобразования числа: {ex.Message}");
                    _parameterValues[parameter.PropertyName] = null;
                }
            };

            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(0, 5),
                Spacing = 10
            };

            panel.Children.Add(label);
            panel.Children.Add(numericUpDown);
            panel.Children.Add(clearButton);

            return panel;
        }

        private Control CreateTextBox(QueryParameter parameter)
        {
            var textBox = new TextBox
            {
                Watermark = parameter.DisplayName,
                Width = 300,  // Увеличил ширину
                Margin = new Thickness(0, 0, 0, 10)  // Добавил отступ снизу
            };

            textBox.LostFocus += (sender, e) =>
            {
                _parameterValues[parameter.PropertyName] = textBox.Text;
            };

            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,  // Выравнивание по центру
                Margin = new Thickness(0, 5)  // Вертикальные отступы
            };
            panel.Children.Add(textBox);
            return panel;
        }

        private Control CreateDatePicker(QueryParameter parameter)
        {
            // Текстовая метка с названием параметра
            var label = new TextBlock
            {
                Text = parameter.DisplayName,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeight.Bold
            };

            // DatePicker
            var datePicker = new DatePicker
            {
                Width = 300,
                Margin = new Thickness(0, 0, 10, 0),
                [!DatePicker.SelectedDateProperty] = new Binding("SelectedDate")
                {
                    Converter = new DateTimeToDateTimeOffsetConverter(),
                    Mode = BindingMode.TwoWay
                }
            };

            // Кнопка очистки
            var clearButton = new Button
            {
                Content = "Очистить",
                Width = 85,
                Margin = new Thickness(0, 0, 0, 0),
                Command = ReactiveCommand.Create(() =>
                {
                    datePicker.SelectedDate = null;
                    _parameterValues[parameter.PropertyName] = null;
                })
            };

            var viewModel = new DatePickerViewModel();
            datePicker.DataContext = viewModel;

            datePicker.SelectedDateChanged += (sender, e) =>
            {
                _parameterValues[parameter.PropertyName] = datePicker.SelectedDate.HasValue
                    ? datePicker.SelectedDate.Value.DateTime
                    : null;
            };

            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(0, 5),
                Spacing = 10 // Расстояние между элементами
            };

            panel.Children.Add(label);
            panel.Children.Add(datePicker);
            panel.Children.Add(clearButton);

            return panel;
        }

        // Вспомогательный класс для привязки данных DatePicker
        private class DatePickerViewModel : ReactiveObject
        {
            private DateTimeOffset? _selectedDate;
            public DateTimeOffset? SelectedDate
            {
                get => _selectedDate;
                set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
            }
        }

        private async Task<Control> CreateComboBox(QueryParameter parameter)
        {
            var comboBox = new ComboBox
            {
                Width = 300,  // Увеличил ширину
                Margin = new Thickness(0, 0, 0, 10),  // Добавил отступ снизу
                PlaceholderText = parameter.DisplayName,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };

            if (parameter.SourceTable != null && parameter.DisplayMember != null && parameter.ValueMember != null)
            {
                var tableDisplayName = AutoPartsStoreTables.TableDefinitions
                    .First(t => t.DbName == parameter.SourceTable).DisplayName;

                var data = await _tablesService.GetTableDataAsync(tableDisplayName);

                comboBox.ItemsSource = data;
                comboBox.ItemTemplate = new FuncDataTemplate<object>((item, scope) =>
                {
                    var textBlock = new TextBlock
                    {
                        Margin = new Thickness(5),  // Отступы внутри элемента
                        Text = item?.GetType().GetProperty(parameter.DisplayMember)?.GetValue(item)?.ToString()
                    };
                    return textBlock;
                });

                comboBox.SelectionChanged += (sender, e) =>
                {
                    _parameterValues[parameter.PropertyName] = comboBox.SelectedItem != null
                        ? comboBox.SelectedItem.GetType().GetProperty(parameter.ValueMember)?.GetValue(comboBox.SelectedItem)
                        : null;
                };
            }

            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 5)
            };
            panel.Children.Add(comboBox);
            return panel;
        }

        public async Task ExecuteQuery()
        {
            if (SelectedQueryVariation == null) return;

            if (!MainWindowViewModel.Instance.IsAuthenticated) return;

            try
            {
                var parameters = new object[SelectedQueryVariation.Parameters.Count];

                for (int i = 0; i < SelectedQueryVariation.Parameters.Count; i++)
                {
                    var paramDef = SelectedQueryVariation.Parameters[i];

                    // Получаем значение параметра из словаря
                    if (!_parameterValues.TryGetValue(paramDef.PropertyName, out var paramValue))
                    {
                        parameters[i] = null;
                        continue;
                    }

                    // Обработка числовых значений
                    if (paramDef.InputType == QueryParameterType.NumericUpDown && paramValue != null)
                    {
                        parameters[i] = Convert.ChangeType(paramValue, paramDef.ParameterType);
                    }

                    // Преобразование типов для ComboBox
                    if (paramDef.InputType == QueryParameterType.ComboBox && paramValue != null)
                    {
                        parameters[i] = ConvertParameterType(paramValue, paramDef.ParameterType);
                    }

                    // Преобразование для DateTime
                    else if (paramDef.ParameterType == typeof(DateTime)
                             && paramValue is string dateString
                             && DateTime.TryParse(dateString, out var date))
                    {
                        parameters[i] = date;
                    }
                    else
                    {
                        parameters[i] = paramValue;
                    }
                }

                // Выполняем запрос
                var query = await SelectedQueryVariation.ExecutionFunction(parameters);
                Data = new ObservableCollection<object>(query);
            }
            catch (Exception ex)
            {
                _currentNotification = CreateNotification("Ошибка", $"Ошибка выполнения запроса: {ex.Message}", NotificationManager, _currentNotification);
            }
        }

        private object ConvertParameterType(object value, Type targetType)
        {
            try
            {
                if (targetType == typeof(int)) return Convert.ToInt32(value);
                if (targetType == typeof(decimal)) return Convert.ToDecimal(value);
                if (targetType == typeof(DateTime)) return Convert.ToDateTime(value);
                return value;
            }
            catch
            {
                return null; // или значение по умолчанию для типа
            }
        }

        private void LoadQueryDefinitions()
        {
            // Здесь заполняем QueryDefinitions списком запросов и их вариаций
            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "1. Поставщики по категории и товару",
                Description = "Получить перечень поставщиков определенной категории, поставляющих указанный вид товара.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "С указанным товаром",
                        Description = "Запрос поставщиков с фильтром по товару.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            return await _queriesService.GetSupplierProvidesProductAsync(productId);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "Товар",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            },
                        }
                    },
                    new QueryVariation
                    {
                        DisplayName = "С указанным объемом за период",
                        Description = "Запрос поставщиков с фильтром по объему поставки за период.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            int value = parameters[1] != null ? int.Parse(parameters[1].ToString()) : 0;
                            DateTime startDate = parameters[2] != null ? (DateTime)parameters[2] : DateTime.MinValue;
                            DateTime endDate = parameters[3] != null ? (DateTime)parameters[3] : DateTime.MaxValue;

                            return await _queriesService.GetSupplierProvidesProductWithValueAsync(productId, value, startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "Товар",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            },
                            new QueryParameter { DisplayName = "Объем поставки", PropertyName = "Value", ParameterType = typeof(int), InputType = QueryParameterType.NumericUpDown },
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "2. Информация о деталях",
                Description = "Получить сведения о конкретном виде деталей: какими поставщиками поставляется, их расценки, время поставки.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "По указанному товару",
                        Description = "Запрос с фильтром по ID товара.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            return await _queriesService.GetDetailProductInfo(productId);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "ID товара",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "3. Покупатели, купившие товар",
                Description = "Получить перечень и общее число покупателей, купивших указанный вид товара за некоторый период либо сделавших покупку товара в объеме, не менее указанного.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос покупателей с фильтром по периоду и минимальному количеству.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            DateTime startDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MinValue;
                            DateTime endDate = parameters[2] != null ? (DateTime)parameters[2] : DateTime.MaxValue;
                            int? minQuantity = parameters[3] != null ? (int)parameters[3] : null;

                            return await _queriesService.GetCustomerBoughtProduct(productId, startDate, endDate, minQuantity);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "Товар",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            },
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Минимальное количество", PropertyName = "MinQuantity", ParameterType = typeof(int), InputType = QueryParameterType.NumericUpDown }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "4. Перечень деталей на складе",
                Description = "Получить перечень, объем и номер ячейки для всех деталей, хранящихся на складе.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "По всем товарам",
                        Description = "Запрос без фильтров.",
                        ExecutionFunction = async (parameters) =>
                        {
                            return await _queriesService.GetCells();
                        },
                        Parameters = new List<QueryParameter>()
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "5. Топ продаваемых деталей и дешевые поставщики",
                Description = "Выведите в порядке возрастания десять самых продаваемых деталей и десять самых «дешевых» поставщиков.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "Топ 10 продаваемых деталей",
                        Description = "Запрос без фильтров.",
                        ExecutionFunction = async (parameters) =>
                        {
                            return await _queriesService.GetTopSellingProductsAsync();
                        },
                        Parameters = new List<QueryParameter>()
                    },
                    new QueryVariation
                    {
                        DisplayName = "Топ 10 дешевых поставщиков",
                        Description = "Фильтр по товару",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            return await _queriesService.GetCheapestSuppliersAsync(productId);
                        },
                        Parameters = new List<QueryParameter>()
                        {
                            new QueryParameter
                            {
                                DisplayName = "ID товара",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "6. Среднее число продаж на месяц",
                Description = "Получите среднее число продаж на месяц по любому виду деталей.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "По указанному товару",
                        Description = "Запрос с фильтром по ID товара.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int productId = parameters[0] != null ? (int)parameters[0] : 0;
                            var res = (List<object>?)await _queriesService.GetAverageMonthlySalesAsync(productId);
                            return res;
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "ID товара",
                                PropertyName = "ProductId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Products",
                                DisplayMember = "ProductName",
                                ValueMember = "ProductId"
                            }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "7. Доля товара поставщика и прибыль магазина",
                Description = "Получите долю товара конкретного поставщика в процентах, деньгах, единицах от всего оборота магазина, прибыль магазина за указанный период.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "Доля товара поставщика",
                        Description = "Запрос с фильтром по ID поставщика и периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            int supplierId = parameters[0] != null ? (int)parameters[0] : 0;
                            DateTime startDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MinValue;
                            DateTime endDate = parameters[2] != null ? (DateTime)parameters[2] : DateTime.MaxValue;

                            return await _queriesService.GetSupplierContributionAsync(supplierId, startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter
                            {
                                DisplayName = "ID поставщика",
                                PropertyName = "SupplierId",
                                ParameterType = typeof(int),
                                InputType = QueryParameterType.ComboBox,
                                SourceTable = "Suppliers",
                                DisplayMember = "SupplierName",
                                ValueMember = "SupplierId"
                            },
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    },
                    new QueryVariation
                    {
                        DisplayName = "Прибыль магазина за период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetStoreProfitAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "8. Накладные расходы в процентах от объема продаж",
                Description = "Получите накладные расходы в процентах от объема продаж.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetOverheadRatioAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "9. Непроданный товар на складе",
                Description = "Получите перечень и общее количество непроданного товара на складе за определенный период и его объем от общего товара в процентах.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetUnsoldStockReportAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "10. Бракованный товар",
                Description = "Получите перечень и общее количество бракованного товара, пришедшего за определенный период и список поставщиков, поставивших товар.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetDefectiveItemsAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "11. Реализованный товар за день",
                Description = "Получите перечень, общее количество и стоимость товара, реализованного за конкретный день.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За указанный день",
                        Description = "Запрос с фильтром по дате.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime saleDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.Now;

                            return await _queriesService.GetDailySalesReportAsync(saleDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата продажи", PropertyName = "SaleDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "12. Кассовый отчет",
                Description = "Получите кассовый отчет за определенный период.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetCashReportAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "13. Скорость оборота денежных средств",
                Description = "Получите скорость оборота денежных средств, вложенных в товар (как быстро товар продается).",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "За период",
                        Description = "Запрос с фильтром по периоду.",
                        ExecutionFunction = async (parameters) =>
                        {
                            DateTime startDate = parameters[0] != null ? (DateTime)parameters[0] : DateTime.MinValue;
                            DateTime endDate = parameters[1] != null ? (DateTime)parameters[1] : DateTime.MaxValue;

                            return await _queriesService.GetProductTurnoverRateAsync(startDate, endDate);
                        },
                        Parameters = new List<QueryParameter>
                        {
                            new QueryParameter { DisplayName = "Дата начала", PropertyName = "StartDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker },
                            new QueryParameter { DisplayName = "Дата окончания", PropertyName = "EndDate", ParameterType = typeof(DateTime), InputType = QueryParameterType.DatePicker }
                        }
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "14. Пустые ячейки на складе",
                Description = "Подсчитайте, сколько пустых ячеек на складе и сколько он сможет вместить товара.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "Все пустые ячейки",
                        Description = "Запрос без фильтров.",
                        ExecutionFunction = async (parameters) =>
                        {
                            return await _queriesService.GetEmptyStorageCellsAsync();
                        },
                        Parameters = new List<QueryParameter>()
                    }
                }
            });

            QueryDefinitions.Add(new QueryDefinition
            {
                DisplayName = "15. Заявки от покупателей на ожидаемый товар",
                Description = "Получите перечень и общее количество заявок от покупателей на ожидаемый товар, подсчитайте, на какую сумму даны заявки.",
                Variations = new List<QueryVariation>
                {
                    new QueryVariation
                    {
                        DisplayName = "Все заявки",
                        Description = "Запрос без фильтров.",
                        ExecutionFunction = async (parameters) =>
                        {
                            return await _queriesService.GetCustomerOrderRequestsAsync();
                        },
                        Parameters = new List<QueryParameter>()
                    }
                }
            });

        }
    }
}
