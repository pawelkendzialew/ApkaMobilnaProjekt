using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models;
using MauiAppMobile.Services.Database;
using MauiAppMobile.Helpers;


namespace MauiAppMobile.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDatabaseService _databaseService;
        private readonly SessionManager _sessionManager;

        public AuthenticationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _sessionManager = SessionManager.Instance;
        }

        public async Task<(bool Success, User User, string Message)> LoginAsync(
            string username, string password)
        {
            try
            {
                var user = await _databaseService.GetUserByUsernameAsync(username);

                if (user == null)
                    return (false, null, "Nieprawidłowa nazwa użytkownika lub hasło");

                if (!user.IsActive)
                    return (false, null, "Konto zostało dezaktywowane");

               // if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                 //   return (false, null, "Nieprawidłowa nazwa użytkownika lub hasło");

                // Aktualizuj ostatnie logowanie
                await _databaseService.UpdateLastLoginAsync(user.Id);
                user.LastLoginAt = DateTime.Now;

                // Zapisz sesję
                _sessionManager.SetCurrentUser(user);

                return (true, user, "Logowanie pomyślne");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd logowania: {ex.Message}");
            }
        }

        public async Task LogoutAsync()
        {
            _sessionManager.ClearSession();
            await Task.CompletedTask;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            return await Task.FromResult(_sessionManager.IsAuthenticated);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await Task.FromResult(_sessionManager.CurrentUser);
        }
    }
}
