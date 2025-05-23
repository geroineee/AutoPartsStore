﻿using AutoParts_Store.UI.Services;
using AutoPartsStore.Data;
using AutoPartsStore.Data.Context;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Notification;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class EditContentViewModel : ViewModelBase
    {
        public object CurrentItem { get; set; }
        public object? AnonymousItem { get; set; }
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

        public EditContentViewModel(Func<AutopartsStoreContext> dbContextFactory) : base(dbContextFactory)
        {

        }

        private readonly Dictionary<string, List<object>> _anonymousItemLists = new(); 

        public void CreateEditControls()
        {
            ControlsContainer.Children.Clear();

            if (string.IsNullOrEmpty(TableName) || CurrentItem == null)
                return;

            var tableDef = AutoPartsStoreTables.TableDefinitions
                .FirstOrDefault(t => t.DisplayName == TableName);

            if (tableDef == null) return;

            bool isNewItem = (OriginalEntity == null); // Определяем, создается ли новая запись

            // Контролы для каждой колонки
            foreach (var column in tableDef.Columns.Where(c => c.IsVisible && c.IsVisibleInEdit))
            {
                // Определяем, является ли колонка редактируемой
                bool isColumnEditable = isNewItem ? true : column.IsEditable;

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
                    Spacing = 250,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                // Текст слева
                panel.Children.Add(new TextBlock
                {
                    Text = column.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 200,
                    FontWeight = FontWeight.Bold
                });

                // Элемент управления справа
                Control inputControl = column.ReferenceTable != null
                    ? CreateReferenceControl(column, isColumnEditable) // ComboBox
                    : CreateBasicControl(column, isColumnEditable);    // Остальные атрибуты

                panel.Children.Add(inputControl);

                // Помещаем панель в Border
                border.Child = panel;

                // Добавляем Border в контейнер
                ControlsContainer.Children.Add(border);
            }
        }


        private Control CreateReferenceControl(TableColumnInfo column, bool isColumnEditable)
        {
            var vm = new ComboBoxViewModel(
                _tablesService,
                column.ReferenceTable,
                column.PropertyName,
                column.ReferenceIdColumn,
                CurrentItem,
                column.ForeignKeyProperty,
                IsNullableForeignKey(CurrentItem.GetType(), column.ForeignKeyProperty)
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
                    string displayText;
                    if (column.ReferenceTable == "Employees" || column.ReferenceTable == "Customers")
                    {
                        displayText = GetFullName(item, column.ReferenceTable);
                    }
                    else
                    {
                        displayText = vm.GetDisplayText(item, column.PropertyName);
                    }

                    return new TextBlock
                    {
                        Text = displayText,
                        FontStyle = displayText == "Не выбрано" ? FontStyle.Italic : FontStyle.Normal,
                    };
                }),
                IsEnabled = isColumnEditable
            };

            comboBox.SelectionChanged += (sender, args) =>
            {
                var comboBox = (ComboBox)sender;
                if (comboBox.SelectedIndex == -1)
                {
                    var fkProperty = CurrentItem.GetType().GetProperty(column.ForeignKeyProperty);
                    if (fkProperty != null)
                    {
                        if (Nullable.GetUnderlyingType(fkProperty.PropertyType) != null || fkProperty.PropertyType == typeof(string))
                        {
                            fkProperty.SetValue(CurrentItem, null);
                        }
                        else
                        {
                            Console.WriteLine($"Свойство {column.ForeignKeyProperty} не допускает null.");
                        }
                    }
                }
                else if (comboBox.SelectedItem != null)
                {
                    var selectedItem = comboBox.SelectedItem;
                    var id = selectedItem?.GetType().GetProperty(column.ReferenceIdColumn)?.GetValue(selectedItem);
                    var fkProperty = CurrentItem.GetType().GetProperty(column.ForeignKeyProperty);
                    fkProperty?.SetValue(CurrentItem, id);
                }
            };

            return comboBox;
        }

        private string GetFullName(object item, string referenceTable)
        {
            if (item == null) return "Не выбрано";

            try
            {
                if (referenceTable == "Employees")
                {
                    var surname = item.GetType().GetProperty("EmployeeSurname")?.GetValue(item)?.ToString();
                    var name = item.GetType().GetProperty("EmployeeName")?.GetValue(item)?.ToString();
                    var patronymic = item.GetType().GetProperty("EmployeePatronymic")?.GetValue(item)?.ToString();

                    return $"{surname} {name} {patronymic}";
                }
                else if (referenceTable == "Customers")
                {
                    var surname = item.GetType().GetProperty("CustomerSurname")?.GetValue(item)?.ToString();
                    var name = item.GetType().GetProperty("CustomerName")?.GetValue(item)?.ToString();
                    var patronymic = item.GetType().GetProperty("CustomerPatronymic")?.GetValue(item)?.ToString();
                    return $"{surname} {name} {patronymic}";
                }

                return item.ToString() ?? "Неизвестное значение";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении полного имени: {ex.Message}");
                return "Ошибка отображения";
            }
        }

        private bool IsNullableForeignKey(Type entityType, string foreignKeyProperty)
        {
            var property = entityType.GetProperty(foreignKeyProperty);
            if (property == null) return false; // Если свойство не найдено, считаем, что оно не nullable

            // Проверяем, является ли тип свойства nullable
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        private Control CreateBasicControl(TableColumnInfo column, bool isColumnEditable)
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
                    IsEnabled = isColumnEditable // Учитываем возможность редактирования
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
                    IsEnabled = isColumnEditable // Учитываем возможность редактирования
                };

                txtBox.AddHandler(InputElement.TextInputEvent, (sender, e) =>
                {
                    if (!isColumnEditable) return; // Не обрабатываем ввод, если поле не редактируемое

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
                    IsEnabled = isColumnEditable // Учитываем возможность редактирования
                };

                var clearButton = new Button
                {
                    Content = "x",
                    Width = 25,
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsEnabled = isColumnEditable, // Учитываем возможность редактирования
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
                IsEnabled = isColumnEditable // Учитываем возможность редактирования
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
                    var tableDef = AutoPartsStoreTables.TableDefinitions.First(t => t.DisplayName == TableName).DbName;
                    await _tablesService.UpdateItemAsync(tableDef, OriginalEntity);

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