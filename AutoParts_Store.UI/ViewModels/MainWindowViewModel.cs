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
        private readonly EditContentViewModel _editVM;
        
        public MainWindowViewModel()
        {
            Instance = this;

            ContentViewModel = new OverviewContentViewModel();

            _queriesVM = new();
            _overviewVM = new();
            _editVM = new();
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
        }
    }
}