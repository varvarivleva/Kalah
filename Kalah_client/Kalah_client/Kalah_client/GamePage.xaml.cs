using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private TcpKalahClient client;
        private int playerNumber;

        public GamePage(TcpKalahClient tcpClient)
        {
            InitializeComponent();
            client = tcpClient;

            client.OnMessageReceived += MessageHandler;
            client.SendMessage("START_GAME");
        }

        private void MessageHandler(string message)
        {
            if (message.StartsWith("BOARD"))
            {
                UpdateBoard(message.Substring(6));
            }
        }

        private void PitButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pit = int.Parse(button.Tag.ToString());
            client.SendMessage($"MOVE,{playerNumber},{pit}");
        }

        private void UpdateBoard(string boardState)
        {
            string[] pits = boardState.Split(',');
            // Обновить UI с состоянием лунок
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            client.Disconnect();
        }
    }
}
