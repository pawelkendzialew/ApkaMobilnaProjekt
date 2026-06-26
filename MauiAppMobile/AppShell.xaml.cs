using MauiAppMobile.Views.Employee;


namespace MauiAppMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rejestruj routing dla nawigacji
            Routing.RegisterRoute("OrderDetails", typeof(OrderDetailsPage));
        }
    }
}