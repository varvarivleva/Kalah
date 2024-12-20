using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    // Класс для страницы логина
    public partial class LoginPage : Page
    {
        private readonly TcpKalahClient _client; // Класс для подключения к серверу

        // Конструктор страницы логина
        public LoginPage()
        {
            InitializeComponent(); // Инициализация компонентов страницы
            _client = new TcpKalahClient(); // Создание экземпляра клиента
            Loaded += LoginPage_Loaded; // Подписка на событие загрузки страницы
            Unloaded += LoginPage_Unloaded; // Подписка на событие выгрузки страницы
        }

        // Обработчик события загрузки страницы
        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived += Client_OnMessageReceived; // Подписка на получение сообщений от сервера
        }

        // Обработчик события выгрузки страницы
        private void LoginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived -= Client_OnMessageReceived; // Отписка от получения сообщений
        }

        // Обработчик кнопки входа
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Authenticate("login"); // Аутентификация с действием "login"
        }

        // Обработчик кнопки регистрации
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Authenticate("register"); // Аутентификация с действием "register"
        }

        // Метод для аутентификации пользователя
        private void Authenticate(string action)
        {
            // Попытка подключения к серверу
            if (!_client.Connect("192.168.200.13", 4563))
            {
                MessageBox.Show("Не удалось подключиться к серверу."); // Вывод сообщения об ошибке
                return;
            }

            // Создание объекта с данными пользователя
            var userData = new
            {
                action, // Действие (login или register)
                username = UsernameBox.Text, // Имя пользователя
                password = PasswordBox.Password, // Пароль
                email = action == "register" ? EmailBox.Text : null // Электронная почта, если действие - регистрация
            };

            // Сериализация данных в JSON
            string jsonData = JsonSerializer.Serialize(userData);
            _client.SendMessage(jsonData); // Отправка данных на сервер
        }

        // Обработчик получения сообщений от сервера
        private void Client_OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<Response>(message, options); // Десериализация ответа сервера
                HandleResponse(response); // Обработка ответа
            });
        }

        // Метод для обработки ответа от сервера
        private void HandleResponse(Response response)
        {
            if (response.Action == "login")
            {
                if (response.Message == "OK")
                {
                    NavigationService.Navigate(new GameModePage(_client)); // Переход на страницу выбора режима игры
                }
                else
                {
                    MessageBox.Show("Неверные учетные данные."); // Уведомление о неверных учетных данных
                }
            }
            else if (response.Action == "register")
            {
                // Уведомление о результате регистрации
                MessageBox.Show(response.Message == "OK" ? "Регистрация прошла успешно. Теперь авторизуйтесь" :
                "Ошибка регистрации. Попробуйте другой логин.");
            }
        }
    }

    // Класс для обработки ответа от сервера
    public class Response
    {
        public string Action { get; set; } // Действие (login или register)
        public string Message { get; set; } // Сообщение от сервера
    }
}
