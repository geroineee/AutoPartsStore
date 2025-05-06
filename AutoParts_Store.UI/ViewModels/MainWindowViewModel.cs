using ReactiveUI;
using System;


namespace AutoParts_Store.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _contentViewModel;

        private string _headerText;

        private bool _isAppThemeLight = false;

        public string AppTheme
        {
            get
            {
                return _isAppThemeLight ? "Light" : "Dark";
            }
        }

        public string HeaderText
        {
            get => _headerText;
            set => this.RaiseAndSetIfChanged(ref _headerText, value);
        }

        public static MainWindowViewModel? Instance { get; set; }

        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        // Страницы
        private readonly QueriesContentViewModel _queriesVM;
        private readonly OverviewContentViewModel _overviewVM;
        private readonly EditContentViewModel _editVM;
        
        public MainWindowViewModel()
        {
            Instance = this;

            _queriesVM = new();
            _overviewVM = new();
            _editVM = new();

            ChangeContent(typeof(OverviewContentViewModel));
        }

        public void ChangeContent(Type viewModelType)
        {
            ContentViewModel = viewModelType switch
            {
                Type view when view == typeof(QueriesContentViewModel) => _queriesVM,
                Type view when view == typeof(OverviewContentViewModel) => _overviewVM,
                Type view when view == typeof(EditContentViewModel) => _editVM,
                _ => throw new ArgumentException("Неподдерживаемый тип окна."),
            };

            switch (viewModelType)
            {
                case Type view when view == typeof(QueriesContentViewModel):
                    ContentViewModel = _queriesVM;
                    HeaderText = "Запросы";
                    break;
                case Type view when view == typeof(OverviewContentViewModel):
                    ContentViewModel = _overviewVM;
                    HeaderText = "Просмотр данных";
                    break;
                case Type view when view == typeof(EditContentViewModel):
                    ContentViewModel = _editVM;
                    HeaderText = "Редактирование записи";
                    break;
            }
        }

        public void SetTheme()
        {
            _isAppThemeLight = !_isAppThemeLight;
            this.RaisePropertyChanged(nameof(AppTheme));
        }
    }
}