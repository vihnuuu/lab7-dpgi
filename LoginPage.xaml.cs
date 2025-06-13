using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApplProject
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = emailBox.Text.Trim();
            string password = passwordBox.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, введіть логін та пароль.");
                return;
            }

            using (var db = new WellnessTrackerNewEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    MessageBox.Show("Користувача не знайдено.");
                    return;
                }

                // Тут використовується перевірка через хеш
                bool isPasswordCorrect = PasswordHelper.VerifyPassword(password, user.PasswordHash);

                if (!isPasswordCorrect)
                {
                    MessageBox.Show("Невірний пароль.");
                    return;
                }

                MessageBox.Show($"Вітаємо, {user.Username}!");

                // Збереження ID користувача для інших сторінок
                App.Current.Properties["UserId"] = user.Id;

                NavigationService?.Navigate(new PageMain()); // Переход куди потрібно
            }
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage()); // Перехід до сторінки реєстрації
        }
    }
}
