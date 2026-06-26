using Microsoft.Extensions.Logging;
using MauiAppMobile.Services;
using MauiAppMobile.ViewModels;
using MauiAppMobile.Views.Administrator;
using MauiAppMobile.Views;
using MauiAppMobile.Services.Authentication;
using MauiAppMobile.Services.Database;
using MauiAppMobile.Services.Orders;
using MauiAppMobile.Services.Timer;
using MauiAppMobile.ViewModels.Authentication;
using MauiAppMobile.ViewModels.Employee;
using MauiAppMobile.Views.Authentication;
using MauiAppMobile.Views.Employee;
using MauiAppMobile.Views.Administrator;


namespace MauiAppMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ===== SERWISY =====
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<IOrderService, OrderService>();
            builder.Services.AddSingleton<IWorkTimerService, WorkTimerService>();

            // ===== VIEWMODELS =====
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<EmployeeDashboardViewModel>();
            builder.Services.AddTransient<OrderDetailsViewModel>();

            // ===== VIEWS =====
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<EmployeeDashboardPage>();
            builder.Services.AddTransient<OrderDetailsPage>();
            builder.Services.AddTransient<AdminDashboardPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}