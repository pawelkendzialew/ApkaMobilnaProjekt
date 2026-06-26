using MauiAppMobile.ViewModels.Employee;

namespace MauiAppMobile.Views.Employee
{
    public partial class EmployeeDashboardPage : ContentPage
    {
        public EmployeeDashboardPage(EmployeeDashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is EmployeeDashboardViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
