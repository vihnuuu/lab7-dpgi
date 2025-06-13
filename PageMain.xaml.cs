using System.Windows;
using System.Windows.Controls;

namespace WpfApplProject
{
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PageTable1());
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PageTable2());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Remove("UserId");
            ((MainWindow)Application.Current.MainWindow).frame1.Navigate(new LoginPage());
        }

    }
}
