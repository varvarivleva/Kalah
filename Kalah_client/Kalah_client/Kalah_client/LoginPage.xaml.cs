using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class LoginPage : Page
    {
        private TcpKalahClient client;

        public LoginPage()
        {
            InitializeComponent();
            client = new TcpKalahClient();  // Создание клиента без параметров
            client.OnMessageReceived += OnServerResponse;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text; // Связь с XAML через x:Name
            string password = PasswordTextBox.Password; // Связь с XAML через x:Name

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Подключаемся к серверу
            bool isConnected = client.Connect("192.168.200.13", 4563); // Указываем IP и порт
            if (!isConnected)
            {
                MessageBox.Show("Unable to connect to the server.");
                return;
            }

            // Отправляем запрос на логин
            string message = $"LOGIN:{username},{password}";
            client.SendMessage(message);
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
            bool isConnected = client.Connect("192.168.200.13", 4563); // Указываем IP и порт
            if (!isConnected)
            {
                MessageBox.Show("Unable to connect to the server.");
                return;
            }

            // Отправляем запрос на регистрацию
            string message = $"REGISTER:{username},{password}";
            client.SendMessage(message);
        }

        private void OnServerResponse(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("LOGIN:OK") || message.StartsWith("REGISTER:OK"))
                {
                    MessageBox.Show("Operation successful!");
                    GameModePage gameModePage = new GameModePage(client);  // Передаем клиента в GameModePage
                    NavigationService.Navigate(gameModePage);  // Навигация к следующей странице
                }
                else if (message.StartsWith("ERROR"))
                {
                    MessageBox.Show($"Server error: {message}");
                }
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            client.Disconnect();
        }
    }
}
