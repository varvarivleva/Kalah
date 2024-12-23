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
            NavigateToGamePage();
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("SET_MODE,PLAYER");
            NavigateToGamePage();
        }

        private void NavigateToGamePage()
        {
            GamePage gamePage = new GamePage(client);  // Передаем клиента на следующую страницу
            this.NavigationService.Navigate(gamePage);  // Навигация с использованием NavigationService
        }
    }
}
