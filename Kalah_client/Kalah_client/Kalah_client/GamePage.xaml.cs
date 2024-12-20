using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private readonly TcpKalahClient _client;
        private int[] _boardState;

        public GamePage(TcpKalahClient client, bool isGameWithComputer)
        {
            InitializeComponent();
            _client = client;
            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;

            InitializeBoard();
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

        private void InitializeBoard()
        {
            _boardState = Enumerable.Repeat(6, 12).ToArray(); // 6 камней в 6 лунках для каждого игрока
            RefreshBoard();
        }

        private void RefreshBoard()
        {
            // Обновляем количество камней на кнопках для обоих игроков
            for (int i = 0; i < 6; i++)
            {
                var button1 = (Button)Player2Pits.Children[i];
                button1.Content = _boardState[i + 6]; // Лунки игрока 2

                var button2 = (Button)Player1Pits.Children[5 - i];
                button2.Content = _boardState[5 - i]; // Лунки игрока 1
            }

            KalahaPlayer1.Text = _boardState[6].ToString(); // Калах игрока 1
            KalahaPlayer2.Text = _boardState[13].ToString(); // Калах игрока 2
        }

        private void UpdateBoard(string boardState)
        {
            _boardState = boardState.Split(',').Select(int.Parse).ToArray();
            RefreshBoard();
        }

        private void PitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Находим индекс лунки в зависимости от того, за какого игрока мы играем
                int pitIndex = -1;

                // Поиск в лунках игрока 2
                if (Player2Pits.Children.Contains(button))
                {
                    pitIndex = Player2Pits.Children.IndexOf(button);
                }
                // Поиск в лунках игрока 1
                else if (Player1Pits.Children.Contains(button))
                {
                    pitIndex = 5 - Player1Pits.Children.IndexOf(button);
                }

                if (pitIndex >= 0)
                {
                    MakeMove(pitIndex);
                }
            }
        }

        private void MakeMove(int pitIndex)
        {
            // Отправка сообщения с выбранной лункой на сервер
            _client.SendMessage($"MOVE:{pitIndex}");
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            // Отправка сообщения о завершении хода на сервер
            _client.SendMessage("END_TURN");
            LogTextBox.AppendText("Ход завершён.\n"); // Для обратной связи
        }
    }
}
