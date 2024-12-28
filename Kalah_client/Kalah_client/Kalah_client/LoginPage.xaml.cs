using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class LoginPage : Page
    {
        private TcpKalahClient _client;

        public LoginPage()
        {
            InitializeComponent();
            _client = new TcpKalahClient(); 
            _client.OnMessageReceived += OnMessageReceived;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Подключаемся к серверу
            bool isConnected = _client.Connect("192.168.200.13", 4563);
            if (!isConnected)
            {
                MessageBox.Show("Unable to connect to the server.");
                return;
            }

            // Отправляем запрос на логин
            string message = $"LOGIN:{username},{password}";
            _client.SendMessage(message);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Подключаемся к серверу
            bool isConnected = _client.Connect("192.168.200.13", 4563); 
            if (!isConnected)
            {
                MessageBox.Show("Unable to connect to the server.");
                return;
            }

            // Отправляем запрос на регистрацию
            string message = $"REGISTER:{username},{password}";
            _client.SendMessage(message);
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("LOGIN:OK") || message.StartsWith("REGISTER:OK"))
                {
                    _client.ClearMessageReceivedHandlers();
                    GameModePage gameModePage = new GameModePage(_client);  // Передаем клиента в GameModePage
                    NavigationService.Navigate(gameModePage);  // Навигация к следующей странице
                }
                else if (message.StartsWith("ERROR"))
                {
                    MessageBox.Show($"Server error: {message}");
                }
            });
        }
    }
}
