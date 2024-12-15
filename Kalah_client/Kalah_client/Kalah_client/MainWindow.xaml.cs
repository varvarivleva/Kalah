using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace KalahClient
{
    public partial class MainWindow : Window
    {
        private Socket _socket;
        private Thread _receiveThread;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
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
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("PLAY_WITH_COMPUTER");
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

                    // Обработка сообщений сервера
                    if (message.StartsWith("START_GAME_WITH_PLAYER"))
                    {
                        Dispatcher.Invoke(() => Log("Игра с игроком началась."));
                    }
                    else if (message.StartsWith("START_GAME_WITH_COMPUTER"))
                    {
                        Dispatcher.Invoke(() => Log("Игра с компьютером началась."));
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
