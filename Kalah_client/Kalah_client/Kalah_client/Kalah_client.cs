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

        // Конструктор, который принимает IP-адрес и порт
        public TcpKalahClient()
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string serverIp, int port)
        {
            try
            {
                _clientSocket.Connect(serverIp, port); // Подключение к серверу
                _receiveThread = new Thread(ReceiveMessages);
                _receiveThread.IsBackground = true;
                _receiveThread.Start(); // Запуск потока для получения сообщений
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
                _clientSocket.Send(data); // Отправка сообщения на сервер
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
                    int bytesRead = _clientSocket.Receive(buffer); // Получение данных от сервера
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message); // Вызов события при получении сообщения
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
            _clientSocket?.Close(); // Закрытие сокета
            _receiveThread?.Interrupt(); // Завершение потока
        }
    }
}
