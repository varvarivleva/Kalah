using System.Windows;
using System.Windows.Controls;
using System.Text.Json;

namespace KalahClient
{
    // Класс для страницы выбора режима игры
    public partial class GameModePage : Page
    {
        private readonly TcpKalahClient _client; // Класс для подключения к серверу

        // Конструктор страницы выбора режима игры
        public GameModePage(TcpKalahClient client)
        {
            InitializeComponent(); // Инициализация компонентов страницы
            _client = client; // Установка клиента
            Loaded += GameModePage_Loaded; // Подписка на событие загрузки страницы
            Unloaded += GameModePage_Unloaded; // Подписка на событие выгрузки страницы
        }

        // Обработчик события загрузки страницы
        private void GameModePage_Loaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived += Client_OnMessageReceived; // Подписка на получение сообщений от сервера
        }

        // Обработчик события выгрузки страницы
        private void GameModePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived -= Client_OnMessageReceived; // Отписка от получения сообщений
        }

        // Обработчик получения сообщений от сервера
        private void Client_OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() => HandleGameModeMessage(message)); // Обработка сообщений в UI потоке
        }

        // Метод для обработки сообщений о выборе режима игры
        private void HandleGameModeMessage(string message)
        {
            if (message == "START_GAME_WITH_PLAYER")
            {
                NavigationService.Navigate(new GamePage(_client, false)); // Переход на игровую страницу для игры с другим игроком
            }
            else if (message == "START_GAME_WITH_COMPUTER")
            {
                NavigationService.Navigate(new GamePage(_client, true)); // Переход на игровую страницу для игры с компьютером
            }
            else if (message.StartsWith("WAITING_FOR_PLAYER"))
            {
                MessageBox.Show("Ожидание соперника..."); // Уведомление о том, что ожидается соперник
            }
        }

        // Обработчик кнопки для игры с игроком
        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(JsonSerializer.Serialize(new { action = "play_with_player" })); // Отправка запроса на игру с игроком
        }

        // Обработчик кнопки для игры с компьютером
        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(JsonSerializer.Serialize(new { action = "play_with_computer" })); // Отправка запроса на игру с компьютером
        }
    }
}
