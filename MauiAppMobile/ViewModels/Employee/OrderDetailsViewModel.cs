using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiAppMobile.Models;
using MauiAppMobile.Models.Enums;
using MauiAppMobile.Services.Orders;
using MauiAppMobile.Services.Authentication;
using MauiAppMobile.Services.Timer;
using MauiAppMobile.ViewModels.Base;

namespace MauiAppMobile.ViewModels.Employee
{
    [QueryProperty(nameof(OrderId), "OrderId")]
    public class OrderDetailsViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authService;
        private readonly IWorkTimerService _timerService;
        private System.Timers.Timer _uiTimer;

        private int _orderId;
        private Order _order;
        private User _currentUser;
        private string _elapsedTimeText;
        private bool _canAccept;
        private bool _canReject;
        private bool _canStart;
        private bool _canComplete;
        private bool _isTimerRunning;

        public OrderDetailsViewModel(
            IOrderService orderService,
            IAuthenticationService authService,
            IWorkTimerService timerService)
        {
            _orderService = orderService;
            _authService = authService;
            _timerService = timerService;

            Title = "Szczegóły Zlecenia";
            ElapsedTimeText = "00:00:00";

            AcceptOrderCommand = new Command(async () => await AcceptOrderAsync(), () => CanAccept);
            RejectOrderCommand = new Command(async () => await RejectOrderAsync(), () => CanReject);
            StartOrderCommand = new Command(async () => await StartOrderAsync(), () => CanStart);
            CompleteOrderCommand = new Command(async () => await CompleteOrderAsync(), () => CanComplete);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Timer dla aktualizacji UI co sekundę
            _uiTimer = new System.Timers.Timer(1000);
            _uiTimer.Elapsed += (s, e) => UpdateElapsedTime();
        }

        #region Properties

        public int OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged();
                _ = LoadOrderAsync();
            }
        }

        public Order Order
        {
            get => _order;
            set
            {
                SetProperty(ref _order, value);
                UpdateCommandStates();
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string ElapsedTimeText
        {
            get => _elapsedTimeText;
            set => SetProperty(ref _elapsedTimeText, value);
        }

        public bool CanAccept
        {
            get => _canAccept;
            set
            {
                SetProperty(ref _canAccept, value);
                ((Command)AcceptOrderCommand).ChangeCanExecute();
            }
        }

        public bool CanReject
        {
            get => _canReject;
            set
            {
                SetProperty(ref _canReject, value);
                ((Command)RejectOrderCommand).ChangeCanExecute();
            }
        }

        public bool CanStart
        {
            get => _canStart;
            set
            {
                SetProperty(ref _canStart, value);
                ((Command)StartOrderCommand).ChangeCanExecute();
            }
        }

        public bool CanComplete
        {
            get => _canComplete;
            set
            {
                SetProperty(ref _canComplete, value);
                ((Command)CompleteOrderCommand).ChangeCanExecute();
            }
        }

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set => SetProperty(ref _isTimerRunning, value);
        }

        #endregion

        #region Commands

        public ICommand AcceptOrderCommand { get; }
        public ICommand RejectOrderCommand { get; }
        public ICommand StartOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand BackCommand { get; }

        #endregion

        #region Methods

        private async Task LoadOrderAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                CurrentUser = await _authService.GetCurrentUserAsync();
                Order = await _orderService.GetOrderByIdAsync(OrderId);

                if (Order == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Błąd", "Nie znaleziono zlecenia", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Sprawdź czy timer już działa
                if (Order.Status == OrderStatus.InProgress && Order.StartedAt.HasValue)
                {
                    _timerService.StartTimer(Order.Id);
                    IsTimerRunning = true;
                    _uiTimer.Start();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Błąd", $"Nie udało się pobrać zlecenia: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateCommandStates()
        {
            if (Order == null || CurrentUser == null)
            {
                CanAccept = false;
                CanReject = false;
                CanStart = false;
                CanComplete = false;
                return;
            }

            bool isAssignedToUser = Order.AssignedEmployeeId == CurrentUser.Id;

            CanAccept = isAssignedToUser && Order.Status == OrderStatus.Sent;
            CanReject = isAssignedToUser && Order.Status == OrderStatus.Sent;
            CanStart = isAssignedToUser && Order.Status == OrderStatus.Accepted;
            CanComplete = isAssignedToUser && Order.Status == OrderStatus.InProgress;
        }

        private async Task AcceptOrderAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Przyjęcie zlecenia",
                "Czy na pewno chcesz przyjąć to zlecenie?",
                "Tak", "Nie");

            if (!confirm)
                return;

            IsBusy = true;

            try
            {
                var success = await _orderService.AcceptOrderAsync(OrderId, CurrentUser.Id);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sukces", "Zlecenie zostało przyjęte", "OK");
                    await LoadOrderAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Błąd", "Nie udało się przyjąć zlecenia", "OK");
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

        private async Task RejectOrderAsync()
        {
            var reason = await Application.Current.MainPage.DisplayPromptAsync(
                "Odrzucenie zlecenia",
                "Podaj powód odrzucenia zlecenia:",
                "OK", "Anuluj",
                placeholder: "Wpisz powód...",
                maxLength: 500);

            if (string.IsNullOrWhiteSpace(reason))
                return;

            IsBusy = true;

            try
            {
                var success = await _orderService.RejectOrderAsync(OrderId, CurrentUser.Id, reason);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sukces", "Zlecenie zostało odrzucone", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Błąd", "Nie udało się odrzucić zlecenia", "OK");
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

        private async Task StartOrderAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Rozpoczęcie zlecenia",
                "Czy na pewno chcesz rozpocząć realizację zlecenia?\nZostanie uruchomiony licznik czasu pracy.",
                "Rozpocznij", "Anuluj");

            if (!confirm)
                return;

            IsBusy = true;

            try
            {
                var success = await _orderService.StartOrderAsync(OrderId, CurrentUser.Id);

                if (success)
                {
                    // ⏱️ URUCHOM TIMER!
                    _timerService.StartTimer(OrderId);
                    IsTimerRunning = true;
                    _uiTimer.Start();

                    await Application.Current.MainPage.DisplayAlert(
                        "Sukces", "Zlecenie rozpoczęte - licznik czasu został uruchomiony", "OK");
                    await LoadOrderAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Błąd", "Nie udało się rozpocząć zlecenia", "OK");
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

        private async Task CompleteOrderAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Zakończenie zlecenia",
                $"Czy na pewno chcesz zakończyć zlecenie?\n\nCzas pracy: {ElapsedTimeText}",
                "Zakończ", "Anuluj");

            if (!confirm)
                return;

            IsBusy = true;

            try
            {
                var success = await _orderService.CompleteOrderAsync(OrderId, CurrentUser.Id);

                if (success)
                {
                    // ⏱️ ZATRZYMAJ TIMER!
                    _timerService.StopTimer(OrderId);
                    IsTimerRunning = false;
                    _uiTimer.Stop();

                    await Application.Current.MainPage.DisplayAlert(
                        "Sukces",
                        $"Zlecenie zakończone!\nCzas pracy: {ElapsedTimeText}",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Błąd", "Nie udało się zakończyć zlecenia", "OK");
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

        private void UpdateElapsedTime()
        {
            if (!IsTimerRunning)
                return;

            var elapsed = _timerService.GetElapsedTime(OrderId);

            // Aktualizuj na głównym wątku UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ElapsedTimeText = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            });
        }

        #endregion

        public void Cleanup()
        {
            _uiTimer?.Stop();
            _uiTimer?.Dispose();
        }
    }
}
