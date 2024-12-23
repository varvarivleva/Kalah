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
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("SET_MODE,COMPUTER");
            StartGame();
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("SET_MODE,PLAYER");
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
