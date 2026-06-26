using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models;

namespace MauiAppMobile.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<(bool Success, User User, string Message)> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<User> GetCurrentUserAsync();
    }
}
