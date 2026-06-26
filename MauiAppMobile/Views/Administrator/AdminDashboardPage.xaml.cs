namespace MauiAppMobile.Views.Administrator
{
    public partial class AdminDashboardPage : ContentPage
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Wylogowanie",
                "Czy na pewno chcesz siê wylogowaæ?",
                "Tak", "Nie");

            if (confirm)
            {
                await Shell.Current.GoToAsync("///Login");
            }
        }
    }
}