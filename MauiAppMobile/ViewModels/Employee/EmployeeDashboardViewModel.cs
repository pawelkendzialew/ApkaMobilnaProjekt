using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using MauiAppMobile.Models;
using MauiAppMobile.Services.Orders;
using MauiAppMobile.Services.Authentication;
using MauiAppMobile.ViewModels.Base;

namespace MauiAppMobile.ViewModels.Employee
{
    public class EmployeeDashboardViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authService;
        private ObservableCollection<Order> _orders;
        private User _currentUser;

        public EmployeeDashboardViewModel(
            IOrderService orderService,
            IAuthenticationService authService)
        {
            _orderService = orderService;
            _authService = authService;

            Orders = new ObservableCollection<Order>();

            RefreshCommand = new Command(async () => await LoadOrdersAsync());
            OrderTappedCommand = new Command<Order>(async (order) => await NavigateToOrderDetails(order));
            LogoutCommand = new Command(async () => await LogoutAsync());

            Title = "Moje Zlecenia";
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand OrderTappedCommand { get; }
        public ICommand LogoutCommand { get; }

        public async Task InitializeAsync()
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            if (IsBusy || CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var orders = await _orderService.GetOrdersForEmployeeAsync(CurrentUser.Id);

                Orders.Clear();
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Błąd", $"Nie udało się pobrać zleceń: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToOrderDetails(Order order)
        {
            if (order == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "OrderId", order.Id }
            };

            await Shell.Current.GoToAsync("OrderDetails", navigationParameter);
        }

        private async Task LogoutAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Wylogowanie", "Czy na pewno chcesz się wylogować?", "Tak", "Nie");

            if (confirm)
            {
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("///Login");
            }
        }
    }
}
