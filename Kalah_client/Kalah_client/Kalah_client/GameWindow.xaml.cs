using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class MainWindow : Window
    {
        private Socket _socket;
        private Thread _receiveThread;
        private KalahGame _game;
        private bool _isGameWithComputer;
        private IGameStrategy _strategy;

        public MainWindow(Socket socket)
        {
            InitializeComponent();
            _socket = socket;
            ConnectToServer(); // Можно убрать, если соединение уже установлено
            _game = new KalahGame(_strategy);
            UpdateBoard();
        }


        private void ConnectToServer()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect("192.168.1.101", 4563);
                Log("Подключено к серверу.");
                _receiveThread = new Thread(ReceiveMessages);
                _receiveThread.Start();
            }
            catch (Exception ex)
            {
                Log($"Ошибка подключения: {ex.Message}");
            }
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("PLAY_WITH_PLAYER");
            _isGameWithComputer = false;
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("PLAY_WITH_COMPUTER");
            _isGameWithComputer = true;
        }

        private void PitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isGameWithComputer)
            {
                MakeMove(sender);
                if (!_game.State.IsGameOver())
                {
                    MakeComputerMove();
                }
            }
            else
            {
                MakeMove(sender);
                if (_game.State.IsGameOver())
                {
                    Log("Игра завершена.");
                }
                else
                {
                    SendMessage($"MOVE:{(sender as Button).Name}");
                }
            }
        }

        private void MakeMove(object sender)
        {
            Button clickedButton = sender as Button;
            int pitIndex = int.Parse(clickedButton.Name.Replace("Pit", "")) - 1;
            _game.MakeMove(pitIndex);
            UpdateBoard();

            if (_game.State.IsGameOver())
            {
                Log("Игра завершена.");
                DisplayWinner();
            }
        }

        private void MakeComputerMove()
        {
            Random rnd = new Random();
            int move;
            do
            {
                move = rnd.Next(0, 6);
            } while (_game.State.Pits[1, move] == 0); // Компьютер выбирает ненулевую лунку

            Log($"Компьютер выбрал лунку {move + 1}.");
            _game.MakeMove(move);
            UpdateBoard();

            if (_game.State.IsGameOver())
            {
                Log("Игра завершена.");
                DisplayWinner();
            }
        }

        private void DisplayWinner()
        {
            if (_game.State.Kalaha[0] > _game.State.Kalaha[1])
            {
                Log("Игрок 1 победил!");
            }
            else if (_game.State.Kalaha[1] > _game.State.Kalaha[0])
            {
                Log("Игрок 2 победил!");
            }
            else
            {
                Log("Ничья!");
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = _socket.Receive(buffer);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() => Log($"Сервер: {message}"));

                    if (message.StartsWith("START_GAME_WITH_PLAYER"))
                    {
                        Dispatcher.Invoke(() => Log("Игра с игроком началась."));
                    }
                    else if (message.StartsWith("MOVE:"))
                    {
                        string pitIndexStr = message.Replace("MOVE:", "");
                        int pitIndex = int.Parse(pitIndexStr);
                        Dispatcher.Invoke(() => _game.MakeMove(pitIndex));
                        Dispatcher.Invoke(() => UpdateBoard());
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Log($"Ошибка: {ex.Message}"));
            }
            finally
            {
                _socket.Close();
            }
        }

        private void UpdateBoard()
        {
            KalahaPlayer1.Text = _game.State.Kalaha[0].ToString();
            KalahaPlayer2.Text = _game.State.Kalaha[1].ToString();

            for (int i = 0; i < 6; i++)
            {
                var player1Button = (Button)FindName($"Pit{i + 1}");
                player1Button.Content = _game.State.Pits[0, i].ToString();

                var player2Button = (Button)FindName($"Pit{i + 7}");
                player2Button.Content = _game.State.Pits[1, i].ToString();
            }
        }

        private void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _socket.Send(data);
        }

        private void Log(string message)
        {
            LogTextBox.Text += $"{message}\n";
        }
    }
}
