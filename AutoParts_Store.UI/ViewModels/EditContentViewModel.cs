using AutoParts_Store.UI.ViewModels;
using AutoPartsStore.Data.Context;
using AutoPartsStore.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using ReactiveUI;
using DynamicData;
using Avalonia.Notification;
using Avalonia.Controls;
using Avalonia.Data;
using System.Collections.Generic;
using Avalonia;
using System.Reflection;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

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
            Margin = new Thickness(10)
        };

        public void CreateEditControls()
        {
            // Очищаем контейнер
            ControlsContainer.Children.Clear();

            if (string.IsNullOrEmpty(TableName) || CurrentItem == null)
                return;

            // Получаем определение таблицы
            var tableDef = AutoPartsStoreTables.TableDefinitions
                .FirstOrDefault(t => t.DisplayName == TableName);

            if (tableDef == null) return;

            // Создаем контролы для каждой колонки
            foreach (var column in tableDef.Columns.Where(c => c.IsVisible))
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal, // Главное изменение!
                    Spacing = 10,
                    Margin = new Thickness(0, 5)
                };

                // Текст слева
                panel.Children.Add(new TextBlock
                {
                    Text = column.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 150 // Фиксируем ширину для выравнивания
                });

                // Элемент управления справа
                Control inputControl = column.ReferenceTable != null
                    ? CreateReferenceControl(column) // ComboBox
                    : CreateBasicControl(column);    // TextBox

                panel.Children.Add(inputControl);
                ControlsContainer.Children.Add(panel);
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
                column.ForeignKeyProperty // Использовать ForeignKeyProperty здесь
            );

            var comboBox = new ComboBox
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                DataContext = vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding("Items"),

                // Привязать SelectedItem к соответствующему свойству FK
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
                        Foreground = displayText == "Не выбрано" ? Brushes.Gray : Brushes.Black
                    };
                })
            };

            // реагировать на выбор и обновлять базовый FK
            comboBox.SelectionChanged += (sender, args) =>
            {
                if (args.AddedItems.Count > 0)
                {
                    var selectedItem = args.AddedItems[0];
                    var id = selectedItem?.GetType().GetProperty(column.ReferenceIdColumn)?.GetValue(selectedItem);

                    // получить свойство для записи
                    var fkProperty = CurrentItem.GetType().GetProperty(column.ForeignKeyProperty);
                    fkProperty?.SetValue(CurrentItem, id);
                }
            };
            return comboBox;
        }


        private Control CreateBasicControl(TableColumnInfo column)
        {
            var property = CurrentItem.GetType().GetProperty(column.PropertyName);
            if (property == null) return new TextBox { IsEnabled = false, Text = "Свойство не найдено" };

            return new TextBox
            {
                Width = 200,
                [!TextBox.TextProperty] = new Binding(column.PropertyName)
                {
                    Source = CurrentItem,
                    Mode = BindingMode.TwoWay
                }
            };
        }

        public async Task SaveChangesAsync()
        {
            if (OriginalEntity == null || CurrentItem == null) return;

            try
            {
                // Копируем изменения из CurrentItem в OriginalEntity
                CopyProperties(CurrentItem, OriginalEntity);

                // Сохраняем оригинал в БД
                await _tablesService.UpdateItemAsync(
                    AutoPartsStoreTables.TableDefinitions.First(t => t.DisplayName == TableName).DbName,
                    OriginalEntity
                );

                _currentNotification = CreateNotification("Успех", "Данные сохранены", NotificationManager, _currentNotification);
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