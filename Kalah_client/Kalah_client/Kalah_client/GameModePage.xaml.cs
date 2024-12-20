using System.Windows;
using System.Windows.Controls;
using System.Text.Json;

namespace KalahClient
{
    public partial class GameModePage : Page
    {
        private readonly TcpKalahClient _client;

        public GameModePage(TcpKalahClient client)
        {
            InitializeComponent();
            _client = client;
            Loaded += GameModePage_Loaded;
            Unloaded += GameModePage_Unloaded;
        }

        private void GameModePage_Loaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void GameModePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _client.OnMessageReceived -= Client_OnMessageReceived;
        }

        private void Client_OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() => HandleGameModeMessage(message));
        }

        private void HandleGameModeMessage(string message)
        {
            if (message == "START_GAME_WITH_PLAYER")
            {
                NavigationService.Navigate(new GamePage(_client, false));
            }
            else if (message == "START_GAME_WITH_COMPUTER")
            {
                NavigationService.Navigate(new GamePage(_client, true));
            }
            else if (message.StartsWith("WAITING_FOR_PLAYER"))
            {
                MessageBox.Show("Ожидание соперника...");
            }
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(JsonSerializer.Serialize(new { action = "play_with_player" }));
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(JsonSerializer.Serialize(new { action = "play_with_computer" }));
        }
    }
}
