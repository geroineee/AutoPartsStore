using AutoPartsStore.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using Avalonia.Notification;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AutoParts_Store.UI.ViewModels
{
    public class EditContentViewModel : ViewModelBase
    {
        public object CurrentItem { get; set; }
        public string TableName { get; set; }
        public object? OriginalEntity { get; set; }

        private INotificationMessage? _currentNotification;
        public INotificationMessageManager NotificationManager { get; } = new NotificationMessageManager();

        public StackPanel ControlsContainer { get; } = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(10),
            Width = 800,
        };

        public void CreateEditControls()
        {
            ControlsContainer.Children.Clear();

            if (string.IsNullOrEmpty(TableName) || CurrentItem == null)
                return;

            var tableDef = AutoPartsStoreTables.TableDefinitions
                .FirstOrDefault(t => t.DisplayName == TableName);

            if (tableDef == null) return;

            // Контролы для каждой колонки
            foreach (var column in tableDef.Columns.Where(c => c.IsVisible && c.IsEditable))
            {
                // Border для обводки
                var border = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 5),
                };

                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 200,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                // Текст слева
                panel.Children.Add(new TextBlock
                {
                    Text = column.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 150,
                    FontWeight = FontWeight.Bold
                });

                // Элемент управления справа
                Control inputControl = column.ReferenceTable != null
                    ? CreateReferenceControl(column) // ComboBox
                    : CreateBasicControl(column);    // Остальные атрибуты


                panel.Children.Add(inputControl);

                // Помещаем панель в Border
                border.Child = panel;

                // Добавляем Border в контейнер
                ControlsContainer.Children.Add(border);
            }
        }

        private Control CreateReferenceControl(TableColumnInfo column)
        {
            var vm = new ComboBoxViewModel(
                _tablesService,
                column.ReferenceTable,
                column.PropertyName,
                column.ReferenceIdColumn,
                CurrentItem,
                column.ForeignKeyProperty
            );

            var comboBox = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Center,
                DataContext = vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding("Items"),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding("SelectedItem")
                {
                    Mode = BindingMode.TwoWay
                },
                ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                {
                    var displayText = vm.GetDisplayText(item, column.PropertyName);
                    return new TextBlock
                    {
                        Text = displayText,
                        FontStyle = displayText == "Не выбрано" ? FontStyle.Italic : FontStyle.Normal,
                        //Foreground = displayText == "Не выбрано" ? Brushes.Gray : Brushes.Black,
                    };
                }),
                IsEnabled = column.IsEditable // Учитываем IsEditable
            };

            comboBox.SelectionChanged += (sender, args) =>
            {
                if (args.AddedItems.Count > 0)
                {
                    var selectedItem = args.AddedItems[0];
                    var id = selectedItem?.GetType().GetProperty(column.ReferenceIdColumn)?.GetValue(selectedItem);

                    var fkProperty = CurrentItem.GetType().GetProperty(column.ForeignKeyProperty);
                    fkProperty?.SetValue(CurrentItem, id);
                }
            };
            return comboBox;
        }

        private Control CreateBasicControl(TableColumnInfo column)
        {
            var property = CurrentItem.GetType().GetProperty(column.PropertyName);
            if (property == null)
            {
                return new TextBox
                {
                    IsEnabled = false,
                    Text = "Свойство не найдено",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }

            // Булевы значения
            if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                return new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(column.PropertyName)
                    {
                        Source = CurrentItem,
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsEnabled = column.IsEditable // Учитываем IsEditable
                };
            }

            // Числовые поля
            if (property.PropertyType == typeof(int) ||
                property.PropertyType == typeof(int?) ||
                property.PropertyType == typeof(decimal) ||
                property.PropertyType == typeof(decimal?) ||
                property.PropertyType == typeof(float) ||
                property.PropertyType == typeof(float?))
            {
                var txtBox = new TextBox
                {
                    Width = 300,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    [!TextBox.TextProperty] = new Binding(column.PropertyName)
                    {
                        Source = CurrentItem,
                        Mode = BindingMode.TwoWay,
                        Converter = new SimpleNumericConverter(property.PropertyType)
                    },
                    IsEnabled = column.IsEditable // Учитываем IsEditable
                };

                txtBox.AddHandler(InputElement.TextInputEvent, (sender, e) =>
                {
                    if (!column.IsEditable) return; // Не обрабатываем ввод, если поле не редактируемое

                    var text = (sender as TextBox)?.Text ?? string.Empty;
                    var newText = text.Insert((sender as TextBox)?.CaretIndex ?? 0, e.Text);
                    var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                    if (!(char.IsDigit(e.Text[0]) ||
                         (e.Text == "-" && (text.Length == 0 || (sender as TextBox)?.CaretIndex == 0)) ||
                         (e.Text == decimalSeparator && !text.Contains(decimalSeparator))))
                    {
                        e.Handled = true;
                    }
                }, RoutingStrategies.Tunnel);

                return txtBox;
            }

            // Обработка всех типов дат
            if (property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?) ||
                property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                var container = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 5
                };

                var datePicker = new DatePicker
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 5, 0),
                    [!DatePicker.SelectedDateProperty] = new Binding(column.PropertyName)
                    {
                        Source = CurrentItem,
                        Mode = BindingMode.TwoWay,
                        Converter = new DateTimeToDateTimeOffsetConverter()
                    },
                    IsEnabled = column.IsEditable // Учитываем IsEditable
                };

                var clearButton = new Button
                {
                    Content = "Очистить",
                    Width = 85,
                    Height = 35,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsEnabled = column.IsEditable, // Учитываем IsEditable
                    Command = ReactiveCommand.Create(() =>
                    {
                        try
                        {
                            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                            {
                                // Для nullable типов
                                property.SetValue(CurrentItem, null);
                                datePicker.SelectedDate = null;
                            }
                            else
                            {
                                _currentNotification = CreateNotification("Информация",
                                    $"Поле '{column.DisplayName}' не поддерживает пустое значение",
                                    NotificationManager,
                                    _currentNotification);
                            }
                        }
                        catch (Exception ex)
                        {
                            _currentNotification = CreateNotification("Ошибка",
                                $"Не удалось сбросить значение: {ex.Message}",
                                NotificationManager,
                                _currentNotification);
                        }

                        this.RaisePropertyChanged(column.PropertyName);
                    })
                };

                container.Children.Add(datePicker);
                container.Children.Add(clearButton);

                return container;
            }

            // Текстовые поля для остальных типов
            return new TextBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Center,
                [!TextBox.TextProperty] = new Binding(column.PropertyName)
                {
                    Source = CurrentItem,
                    Mode = BindingMode.TwoWay
                },
                IsEnabled = column.IsEditable // Учитываем IsEditable
            };
        }

        public async Task SaveChangesAsync()
        {
            if (CurrentItem == null) return;

            try
            {
                if (OriginalEntity == null)
                {
                    var tableDef = AutoPartsStoreTables.TableDefinitions.First(t => t.DisplayName == TableName);

                    await _tablesService.AddNewItemAsync(tableDef.DbName, CurrentItem);

                    _currentNotification = CreateNotification("Успех", "Новая запись создана", NotificationManager, _currentNotification);
                }
                else
                {
                    CopyProperties(CurrentItem, OriginalEntity);
                    await _tablesService.UpdateItemAsync(
                        AutoPartsStoreTables.TableDefinitions.First(t => t.DisplayName == TableName).DbName,
                        OriginalEntity
                    );

                    _currentNotification = CreateNotification("Успех", "Данные сохранены", NotificationManager, _currentNotification);
                }
                Cancel(typeof(OverviewContentViewModel));
            }
            catch (DbUpdateException ex)
            {
                _currentNotification = CreateNotification("Ошибка", ex.InnerException?.Message ?? ex.Message, NotificationManager, _currentNotification);
            }
            catch (Exception ex)
            {
                _currentNotification = CreateNotification("Ошибка", ex.Message, NotificationManager, _currentNotification);
            }
        }


        public void Cancel(Type typeVM)
        {
            MainWindowViewModel.Instance.ChangeContent(typeVM);
            if (MainWindowViewModel.Instance.ContentViewModel is OverviewContentViewModel overviewVM)
            {
                overviewVM.CurrentTable = TableName;
                _ = overviewVM.LoadTableDataAsync();
            }
        }


        private void CopyProperties(object source, object target)
        {
            var properties = source.GetType().GetProperties();
            foreach (var prop in properties.Where(p => p.CanRead && p.CanWrite))
            {
                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }
    }
}