using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private TcpKalahClient _client;
        private bool _isPlayerTurn;

        public GamePage(TcpKalahClient tcpClient)
        {
            InitializeComponent();
            _client = tcpClient;
            _client.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("BOARD_STATE"))
            {
                UpdateBoardState(message);
            }
            else if (message.StartsWith("YOUR_TURN"))
            {
                UpdateTurn("TURN:0");
            }
            else if (message.StartsWith("WAIT_TURN"))
            {
                UpdateTurn("TURN:1");
            }
            else if (message.StartsWith("GAME_OVER"))
            {
                HandleGameOver(message);
            }
            else if (message.StartsWith("ERROR"))
            {
                MessageBox.Show("Ошибка: " + message.Substring(6));
            }
        }


        private void UpdateBoardState(string state)
        {
            string[] parts = state.Substring("BOARD_STATE:".Length).Split(',');

            Dispatcher.Invoke(() =>
            {
                // Обновляем элементы интерфейса
                PlayerKalaha.Content = "Kalah: " + parts[6];
                OpponentKalaha.Content = "Kalah: " + parts[13];

                // Лунки игрока
                PlayerPit0.Content = parts[0];
                PlayerPit1.Content = parts[1];
                PlayerPit2.Content = parts[2];
                PlayerPit3.Content = parts[3];
                PlayerPit4.Content = parts[4];
                PlayerPit5.Content = parts[5];

                // Лунки соперника
                OpponentPit0.Content = parts[7];
                OpponentPit1.Content = parts[8];
                OpponentPit2.Content = parts[9];
                OpponentPit3.Content = parts[10];
                OpponentPit4.Content = parts[11];
                OpponentPit5.Content = parts[12];
            });
        }

        private void UpdateTurn(string message)
        {
            string turn = message.Substring("TURN:".Length).Trim();
            _isPlayerTurn = (turn == "0");

            Dispatcher.Invoke(() =>
            {
                TurnText.Text = _isPlayerTurn ? "Ваш ход" : "Ход соперника";
            });
        }

        private void HandleGameOver(string message)
        {
            string winner = message.Substring("GAME_OVER:".Length).Trim();

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Игра окончена. Победитель: {winner}");
                NavigateToResultPage(); // Возврат к выбору режима
            });
        }

        private void PitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isPlayerTurn)
            {
                MessageBox.Show("Сейчас не ваш ход!");
                return;
            }

            var button = (Button)sender;
            int pitIndex = int.Parse(button.Tag.ToString());

            _client.SendMessage($"MOVE,{pitIndex}");
        }
        private void NavigateToResultPage()
        {
            ResultPage resultPage = new ResultPage(_client);
            this.NavigationService.Navigate(resultPage);
        }
    }
}
