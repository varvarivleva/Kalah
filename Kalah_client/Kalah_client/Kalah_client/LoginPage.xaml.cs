using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class LoginPage : Page
    {
        private readonly TcpKalahClient _client;

        public LoginPage()
        {
            InitializeComponent();
            _client = new TcpKalahClient();
            Loaded += LoginPage_Loaded;
            Unloaded += LoginPage_Unloaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void LoginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived -= Client_OnMessageReceived;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Authenticate("login");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Authenticate("register");
        }

        private void Authenticate(string action)
        {
            if (!_client.Connect("192.168.200.13", 4563))
            {
                MessageBox.Show("Не удалось подключиться к серверу.");
                return;
            }

            var userData = new
            {
                action,
                username = UsernameBox.Text,
                password = PasswordBox.Password,
                email = action == "register" ? EmailBox.Text : null
            };

            string jsonData = JsonSerializer.Serialize(userData);
            _client.SendMessage(jsonData);
        }

        private void Client_OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<Response>(message, options);
                HandleResponse(response);
            });
        }

        private void HandleResponse(Response response)
        {
            if (response.Action == "login")
            {
                if (response.Message == "OK")
                {
                    NavigationService.Navigate(new GameModePage(_client));
                }
                else
                {
                    MessageBox.Show("Неверные учетные данные.");
                }
            }
            else if (response.Action == "register")
            {
                MessageBox.Show(response.Message == "OK" ? "Регистрация прошла успешно. Теперь авторизуйтесь" :
                "Ошибка регистрации. Попробуйте другой логин.");
            }
        }
    }

    public class Response
    {
        public string Action { get; set; }
        public string Message { get; set; }
    }
}
