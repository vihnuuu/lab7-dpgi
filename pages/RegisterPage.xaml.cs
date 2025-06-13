using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApplProject.pages;

namespace WpfApplProject
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameBox.Text.Trim();
            string email = emailBox.Text.Trim();
            string password = passwordBox.Password;
            string confirmPassword = confirmPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Паролі не співпадають.");
                return;
            }

            using (var db = new WellnessTrackerNewEntities())
            {
                if (db.Users.Any(u => u.Email == email))
                {
                    MessageBox.Show("Користувач з таким email вже існує.");
                    return;
                }

                var newUser = new Users
                {
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHelper.ComputeSha256Hash(password)
                };

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            MessageBox.Show("Реєстрація пройшла успішно!");
            NavigationService?.Navigate(new LoginPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
