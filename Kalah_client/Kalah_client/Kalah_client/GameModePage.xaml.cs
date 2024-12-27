using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GameModePage : Page
    {
        private TcpKalahClient client;

        public GameModePage(TcpKalahClient tcpClient)
        {
            InitializeComponent();
            client = tcpClient;
            client.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("WAITING_FOR_PLAYER"))
            {
                WatingForPlayer(message);
            }
            else if (message.StartsWith("ERROR"))
            {
                MessageBox.Show("Ошибка: " + message.Substring(6));
            }
        }

        private void WatingForPlayer(string message)
        {
            MessageBox.Show("Ожидаем подключения второго игрока");
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("SET_MODE:COMPUTER");
            StartGame();
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("SET_MODE:PLAYER");
            StartGame();
        }

        private void StartGame()
        {
            client.SendMessage("PLAY");
            NavigateToGamePage();
        }

        private void NavigateToGamePage()
        {
            GamePage gamePage = new GamePage(client);
            this.NavigationService.Navigate(gamePage);
        }
    }
}
