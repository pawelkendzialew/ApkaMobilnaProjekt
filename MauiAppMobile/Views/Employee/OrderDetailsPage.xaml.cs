using MauiAppMobile.ViewModels.Employee;

namespace MauiAppMobile.Views.Employee
{
	public partial class OrderDetailsPage : ContentPage
	{
        public OrderDetailsPage(OrderDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}