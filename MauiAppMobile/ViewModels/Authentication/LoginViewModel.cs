using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiAppMobile.Models.Enums;
using MauiAppMobile.Services.Authentication;
using MauiAppMobile.ViewModels.Base;

namespace MauiAppMobile.ViewModels.Authentication
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private string _username;
        private string _password;
        private UserRole _selectedRole = UserRole.Employee;

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
            Title = "Logowanie";
            LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public UserRole SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Błąd", "Wprowadź nazwę użytkownika i hasło", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                var (success, user, message) = await _authService.LoginAsync(Username, Password);

                if (success)
                {
                    // Sprawdź czy rola się zgadza
                    if (user.Role != SelectedRole)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Błąd", "Wybrano nieprawidłową rolę", "OK");
                        return;
                    }

                    // Nawigacja w zależności od roli
                    if (user.Role == UserRole.Employee)
                    {
                        await Shell.Current.GoToAsync("///EmployeeDashboard");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("///AdminDashboard");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Błąd", $"Wystąpił błąd: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
