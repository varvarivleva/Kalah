using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            // Логика авторизации пользователя
            if (ValidateLogin(username, password))
            {
                // Переход к выбору режима игры
                NavigationService.Navigate(new GameModePage());
            }
            else
            {
                MessageBox.Show("Неверные учетные данные.");
            }
        }

        private bool ValidateLogin(string username, string password)
        {
            // Имитация проверки (можно заменить на вызов сервера)
            return username == "admin" && password == "1234";
        }
    }
}
