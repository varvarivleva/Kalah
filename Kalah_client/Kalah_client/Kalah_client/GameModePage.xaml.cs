using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GameModePage : Page
    {
        private TcpKalahClient _client;

        public GameModePage(TcpKalahClient tcpClient)
        {
            InitializeComponent();
            _client = tcpClient;
            _client.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("WAITING_FOR_PLAYER"))
            {
                HendleWatingForPlayer(message);
            }
            else if (message.StartsWith("ERROR"))
            {
                MessageBox.Show("Ошибка: " + message.Substring(6));
            }
        }

        private void HendleWatingForPlayer(string message)
        {
            MessageBox.Show("Ожидаем подключения второго игрока");
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage("SET_MODE:COMPUTER");
            StartGame();
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage("SET_MODE:PLAYER");
            StartGame();
        }

        private void StartGame()
        {
            _client.SendMessage("PLAY");
            NavigateToGamePage();
        }

        private void NavigateToGamePage()
        {
            _client.ClearMessageReceivedHandlers();
            GamePage gamePage = new GamePage(_client);
            this.NavigationService.Navigate(gamePage);
        }
    }
}
