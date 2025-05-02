using ReactiveUI;
using System;


namespace AutoParts_Store.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static MainWindowViewModel Instance { get; set; }

        private ViewModelBase _contentViewModel;
        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        // Страницы
        private readonly QueriesContentViewModel _queriesVM;
        private readonly OverviewContentViewModel _overviewVM;
        
        public MainWindowViewModel()
        {
            Instance = this;

            ContentViewModel = new ViewModelBase();

            _queriesVM = new();
            _overviewVM = new();
        }

        public void ChangeContent(Type viewModelType)
        {
            ContentViewModel = viewModelType switch
            {
                Type queriesContentVM when queriesContentVM == typeof(QueriesContentViewModel) => _queriesVM,
                Type overviewContentVM when overviewContentVM == typeof(OverviewContentViewModel) => _overviewVM,
                _ => throw new ArgumentException("Неподдерживаемый тип окна."),
            };
        }
    }
}