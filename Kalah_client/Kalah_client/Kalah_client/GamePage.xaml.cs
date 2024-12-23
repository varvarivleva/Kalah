using System;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private TcpKalahClient _client; // Сохраняем TcpKalahClient
        private KalahGame _game;
        private bool _isPlayerTurn=true;

        // Конструктор, который принимает TcpKalahClient
        public GamePage(TcpKalahClient tcpClient)
        {
            InitializeComponent();
            _client = tcpClient; // Инициализируем TcpKalahClient
            _client.OnMessageReceived += OnMessageReceived; // Подписываемся на получение сообщений
            _game = new KalahGame();
        }

        private void OnMessageReceived(string message)
        {
            // Обработка состояния доски, полученного от сервера
            if (message.StartsWith("BOARD_STATE"))
            {
                UpdateBoardState(message);
            }
            else if (message.StartsWith("ERROR"))
            {
                MessageBox.Show("Ошибка: " + message.Substring(6));
            }
            UpdateTurnStatus();
        }

        // Обновляем UI с состоянием доски
        private void UpdateBoardState(string state)
        {
            string[] parts = state.Split(',');
            parts[0] = parts[0].Trim('B','O','A','R','D','_','S','T','A','T','E',':');

            // Обновляем элементы UI через Dispatcher, чтобы избежать ошибок многозадачности
            Dispatcher.Invoke(() =>
            {
                // Обновляем калахи
                PlayerKalaha.Content = "Kalah: " + parts[6]; // Игрок 1
                OpponentKalaha.Content = "Kalah: " + parts[13]; // Игрок 2

                // Обновляем лунки для игрока 1
                PlayerPit0.Content = parts[0];
                PlayerPit1.Content = parts[1];
                PlayerPit2.Content = parts[2];
                PlayerPit3.Content = parts[3];
                PlayerPit4.Content = parts[4];
                PlayerPit5.Content = parts[5];

                // Обновляем лунки для игрока 2
                OpponentPit0.Content = parts[7];
                OpponentPit1.Content = parts[8];
                OpponentPit2.Content = parts[9];
                OpponentPit3.Content = parts[10];
                OpponentPit4.Content = parts[11];
                OpponentPit5.Content = parts[12];
            });
        }

        private void PitButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int pitIndex = int.Parse(button.Tag.ToString());

            // Отправляем запрос на сервер
            _client.SendMessage($"MOVE,{pitIndex}");

            // Обновляем плашку для хода
            UpdateTurnStatus();

            // Ожидаем хода противника (или компьютера)
            WaitForNextMove();
        }

        // Метод для начала игры, подключаемся к серверу
        public void StartGame(string serverIp, int serverPort)
        {
            if (_client.Connect(serverIp, serverPort))
            {
                _client.SendMessage("PLAY");
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к серверу.");
            }
        }

        private void UpdateTurnStatus()
        {
            Dispatcher.Invoke(() =>
            {
                // Плашка "Ваш ход" или "Ход соперника"
                if (_isPlayerTurn)
                {
                    TurnText.Text = "Ваш ход";
                }
                else
                {
                    TurnText.Text = "Ход соперника";
                }
            });
        }

        private void WaitForNextMove()
        {
            // Ожидание хода от серверной логики
            // Это будет выполнять метод, который будет слушать сервер
            // и обновлять статус в зависимости от хода.
        }
    }
}
