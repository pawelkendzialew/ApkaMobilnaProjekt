using MauiAppMobile.Models.Enums;
using MauiAppMobile.ViewModels.Authentication;

namespace MauiAppMobile.Views.Authentication
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnEmployeeRoleSelected(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel vm)
            {
                vm.SelectedRole = UserRole.Employee;
            }
        }

        private void OnAdminRoleSelected(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel vm)
            {
                vm.SelectedRole = UserRole.Administrator;
            }
        }
    }
}