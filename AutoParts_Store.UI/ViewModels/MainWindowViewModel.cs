using AutoParts_Store.UI.Services;
using AutoPartsStore.Data.Context;
using ReactiveUI;
using System;


namespace AutoParts_Store.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _contentViewModel;
        private readonly IAuthenticationService _authenticationService;
        private bool _isAuthenticated = false;
        private LoginContentViewModel _loginVM;

        private string _headerText;
        private bool _isAppThemeLight = false;

        private string _loginedUsername;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set => this.RaiseAndSetIfChanged(ref _isAuthenticated, value);
        }

        public string LoginedUsername
        {
            get => _loginedUsername;
            set
            {
                string newMessage = $"Вы вошли как: {value}";
                this.RaiseAndSetIfChanged(ref _loginedUsername, newMessage);
            }
        }

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

        public MainWindowViewModel(IAuthenticationService authenticationService, Func<AutopartsStoreContext> dbContextFactory) : base(dbContextFactory)
        {
            Instance = this;

            _authenticationService = authenticationService;
            _loginVM = new LoginContentViewModel(_authenticationService, this);
            _queriesVM = new QueriesContentViewModel(dbContextFactory);
            _overviewVM = new OverviewContentViewModel(dbContextFactory);
            _editVM = new EditContentViewModel(dbContextFactory);

            ChangeContent(typeof(LoginContentViewModel));
        }

        public void ShowMainContent()
        {
            IsAuthenticated = true;
            ChangeContent(typeof(QueriesContentViewModel));
        }

        public void ChangeContent(Type viewModelType)
        {
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
                case Type view when view == typeof(LoginContentViewModel):
                    IsAuthenticated = false;
                    ContentViewModel = _loginVM;
                    HeaderText = "Вход в систему";
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