using AutoParts_Store.UI.Services;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.ViewModels
{
    public class LoginContentViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private readonly IAuthenticationService _authenticationService;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public LoginContentViewModel(IAuthenticationService authenticationService, MainWindowViewModel mainWindowViewModel)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService)); //Add not null check
            _mainWindowViewModel = mainWindowViewModel;
        }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public async Task LoginAsync()
        {
            // Use _authenticationService here.  It should no longer be null
            bool authenticated = await _authenticationService.AuthenticateAsync(Username, Password);
            if (authenticated)
            {
                ErrorMessage = "";
                _mainWindowViewModel.ShowMainContent(); // Notify MainWindow to switch content
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
    }

}
