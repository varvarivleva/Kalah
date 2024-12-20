using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private readonly TcpKalahClient _client;

        public GamePage(TcpKalahClient client, bool isGameWithComputer)
        {
            InitializeComponent();
            _client = client;
            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived -= Client_OnMessageReceived;
        }

        private void Client_OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() => HandleGameMessage(message));
        }

        private void HandleGameMessage(string message)
        {
            if (message.StartsWith("BOARD:"))
            {
                UpdateBoard(message.Substring(6));
            }
            else if (message.StartsWith("GAME_OVER:"))
            {
                MessageBox.Show($"Игра завершена. Результат: {message.Substring(10)}");
                NavigationService.Navigate(new ResultPage(_client));
            }
            else if (message == "INVALID_MOVE")
            {
                MessageBox.Show("Недопустимый ход.");
            }
        }

        private void UpdateBoard(string boardState)
        {
            // Логика обновления игрового поля
        }

        private void MakeMove(int pitIndex)
        {
            _client.SendMessage($"MOVE:{pitIndex}");
        }

        private void EndGameButton_Click(object sender, RoutedEventArgs e)
        {
            _client.Disconnect();
            NavigationService.Navigate(new GameModePage(_client));
        }
    }
}
