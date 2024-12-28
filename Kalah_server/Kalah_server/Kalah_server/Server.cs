// Добавьте пространство имен для работы с Database
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Kalah_server
{
    class KalahServer
    {
        private static List<Socket> clients = new List<Socket>();
        private static Dictionary<Socket, string> clientModes = new Dictionary<Socket, string>(); // Режимы клиентов
        private static Dictionary<Socket, KalahGame> games = new Dictionary<Socket, KalahGame>(); // Игры
        private static Dictionary<Socket, GameStrategy> strategies = new Dictionary<Socket, GameStrategy>(); // Стратегии
        private static Dictionary<Socket, Socket> playerPairs = new Dictionary<Socket, Socket>(); // Пары игроков
        private static Dictionary<Socket, int> playerScore = new Dictionary<Socket, int>(); // Счет игрока
        private static Dictionary<Socket, string> playerName = new Dictionary<Socket, string>(); // Имя игрока на сокете
        private static Queue<Socket> waitingRoom = new Queue<Socket>(); // Очередь ожидания

        private static object lockObj = new object();

        static void Main()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 4563)); // Порт для подключения
            serverSocket.Listen(10); // Максимум 10 ожидающих подключений
            Console.WriteLine("Сервер запущен, ожидает подключения...");

            try
            {
                while (true)
                {
                    Socket clientSocket = serverSocket.Accept(); // Ожидаем подключения клиента
                    Console.WriteLine("Новый клиент подключен.");
                    Thread clientThread = new Thread(() => HandleClient(clientSocket)); // Новый поток для обработки клиента
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                serverSocket.Close();
            }
        }

        // Обработка клиента
        static void HandleClient(Socket clientSocket)
        {
            lock (lockObj)
            {
                clients.Add(clientSocket); // Добавляем клиента в список
            }

            try
            {
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = clientSocket.Receive(buffer); // Получаем данные от клиента
                    if (bytesRead == 0) break; // Если соединение закрыто, выходим

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Получено от клиента: {receivedData}");

                    // Обрабатываем сообщение в зависимости от его типа
                    if (receivedData.StartsWith("REGISTER"))
                    {
                        HandleRegisterRequest(clientSocket, receivedData);
                    }
                    else if (receivedData.StartsWith("LOGIN"))
                    {
                        HandleLoginRequest(clientSocket, receivedData);
                    }
                    else if (receivedData.StartsWith("MOVE"))
                    {
                        HandleMoveRequest(clientSocket, receivedData);
                    }
                    else if (receivedData.StartsWith("PLAY"))
                    {
                        HandlePlayRequest(clientSocket, receivedData);
                    }
                    else if (receivedData.StartsWith("SET_MODE"))
                    {
                        HandleSetModeRequest(clientSocket, receivedData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
                clients.Remove(clientSocket);
            }
            finally
            {
                lock (lockObj)
                {
                    clients.Remove(clientSocket); // Удаляем клиента из списка
                    if (playerPairs.ContainsKey(clientSocket))
                    {
                        Socket opponent = playerPairs[clientSocket];
                        playerPairs.Remove(clientSocket);
                        playerPairs.Remove(opponent);
                        SendMessage(opponent, "PLAYER_DISCONNECTED");
                    }
                    if (games.ContainsKey(clientSocket))
                    {
                        games.Remove(clientSocket);
                    }
                }
                clientSocket.Close();
            }
        }

        // Обработка запроса на регистрацию
        static void HandleRegisterRequest(Socket clientSocket, string request)
        {
            string[] parts = request.Substring("REGISTER:".Length).Split(',');
            string username = parts[0];
            string password = parts[1];

            if (UserDatabase.UserExists(username))
            {
                SendMessage(clientSocket, "ERROR: Username already exists.");
            }
            else
            {
                UserDatabase.AddUser(username, password); // Сохраняем пользователя в базе данных
                SendMessage(clientSocket, "REGISTER:OK");
                playerName[clientSocket] = username;
            }
        }

        // Обработка запроса на логин
        static void HandleLoginRequest(Socket clientSocket, string request)
        {
            string[] parts = request.Substring("LOGIN:".Length).Split(',');
            string username = parts[0];
            string password = parts[1];

            if (UserDatabase.Authorize(username, password))
            {
                SendMessage(clientSocket, "LOGIN:OK");
                playerName[clientSocket] = username;
            }
            else
            {
                SendMessage(clientSocket, "ERROR: Invalid username or password.");
            }
        }

        // Обработка запроса на ход
        static void HandleMoveRequest(Socket clientSocket, string request)
        {
            if (!strategies.ContainsKey(clientSocket))
            {
                SendMessage(clientSocket, "ERROR: Set game mode first.");
                return;
            }

            GameStrategy strategy = strategies[clientSocket];
            strategy.ProcessMove(clientSocket, request, games, playerPairs, playerScore, playerName, SendMessage);
        }

        // Обработка запроса на выбор режима игры
        static void HandleSetModeRequest(Socket clientSocket, string request)
        {
            string[] parts = request.Split(':');
            string mode = parts[1];

            if (mode == "COMPUTER")
            {
                clientModes[clientSocket] = "COMPUTER";
                strategies[clientSocket] = new AIPlayerGameStrategy(); // Устанавливаем стратегию
                SendMessage(clientSocket, "SET_MODE:OK");
                Console.WriteLine($"Клиент {clientSocket.RemoteEndPoint} выбрал режим: Игра с компьютером.");
            }
            else if (mode == "PLAYER")
            {
                clientModes[clientSocket] = "PLAYER";
                strategies[clientSocket] = new NetworkGameStrategy(); // Устанавливаем стратегию
                SendMessage(clientSocket, "SET_MODE:OK");
                Console.WriteLine($"Клиент {clientSocket.RemoteEndPoint} выбрал режим: Игра с другим игроком.");
            }
            else
            {
                SendMessage(clientSocket, "ERROR: Invalid game mode.");
            }
        }

        // Обработка запроса на игру
        static void HandlePlayRequest(Socket clientSocket, string request)
        {
            if (!strategies.ContainsKey(clientSocket))
            {
                SendMessage(clientSocket, "ERROR: Set game mode first.");
                return;
            }

            string mode = clientModes[clientSocket];
            if (mode == "COMPUTER")
            {
                KalahGame game = new KalahGame();
                games[clientSocket] = game;
                SendMessage(clientSocket, "BOARD_STATE:" + game.GetBoardState());
                SendMessage(clientSocket, "YOUR_TURN");
            }
            else if (mode == "PLAYER")
            {
                lock (lockObj)
                {
                    if (waitingRoom.Count > 0)
                    {
                        // Найти оппонента в очереди ожидания
                        Socket opponent = waitingRoom.Dequeue();

                        KalahGame game = new KalahGame();
                        games[clientSocket] = game;
                        games[opponent] = game;

                        playerPairs[clientSocket] = opponent;
                        playerPairs[opponent] = clientSocket;

                        // Определить, кто ходит первым
                        Random random = new Random();
                        Socket firstPlayer = random.Next(2) == 0 ? clientSocket : opponent;
                        Socket secondPlayer = firstPlayer == clientSocket ? opponent : clientSocket;

                        SendMessage(firstPlayer, "YOUR_TURN");
                        SendMessage(secondPlayer, "WAIT_TURN");

                        SendMessage(firstPlayer, "BOARD_STATE:" + game.GetBoardStateForPlayer(1)); // Для игрока 1
                        SendMessage(secondPlayer, "BOARD_STATE:" + game.GetBoardStateForPlayer(2)); // Для игрока 2
                    }
                    else
                    {
                        // Добавить в очередь ожидания
                        waitingRoom.Enqueue(clientSocket);
                        SendMessage(clientSocket, "WAITING_FOR_PLAYER");
                    }
                }
            }
        }

        // Отправка сообщения клиенту
        static void SendMessage(Socket client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data);
        }
    }
}
