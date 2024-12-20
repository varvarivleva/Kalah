using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace KalahClient
{
    public class TcpKalahClient
    {
        private Socket _clientSocket;
        private Thread _receiveThread;

        public event Action<string> OnMessageReceived;

        public bool Connect(string serverIp, int port)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(serverIp, port);
                _receiveThread = new Thread(ReceiveMessages);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _clientSocket.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = _clientSocket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message);
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Соединение с сервером потеряно.");
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            _clientSocket?.Close();
            _receiveThread?.Interrupt();
        }
    }
}
