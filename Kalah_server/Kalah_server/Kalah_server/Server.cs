using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Kalah_server
{
    class KalahServer
    {
        private static List<Socket> clients = new List<Socket>();
        private static Dictionary<string, string> users = new Dictionary<string, string>(); // Пользователи
        private static Dictionary<Socket, string> clientModes = new Dictionary<Socket, string>(); // Режимы клиентов
        private static Dictionary<Socket, KalahGame> games = new Dictionary<Socket, KalahGame>(); // Игры
        private static Dictionary<Socket, Socket> playerPairs = new Dictionary<Socket, Socket>(); // Пары игроков

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
            string[] parts = request.Split(',');
            string username = parts[1];
            string password = parts[2];

            if (users.ContainsKey(username))
            {
                SendMessage(clientSocket, "ERROR: Username already exists.");
            }
            else
            {
                users[username] = password;
                SendMessage(clientSocket, "SUCCESS:REGISTER");
            }
        }

        // Обработка запроса на логин
        static void HandleLoginRequest(Socket clientSocket, string request)
        {
            string[] parts = request.Split(',');
            string username = parts[1];
            string password = parts[2];

            if (users.ContainsKey(username) && users[username] == password)
            {
                SendMessage(clientSocket, "SUCCESS:LOGIN");
            }
            else
            {
                SendMessage(clientSocket, "ERROR: Invalid username or password.");
            }
        }

        static void HandleMoveRequest(Socket clientSocket, string request)
        {
            // Проверяем, существует ли объект игры для клиента
            if (!games.ContainsKey(clientSocket))
            {
                // Если игры нет, создаем новую
                KalahGame newGame = new KalahGame();
                games[clientSocket] = newGame;

                // Отправляем начальное состояние доски
                SendMessage(clientSocket, "BOARD_STATE:" + newGame.GetBoardState());
                SendMessage(clientSocket, "TURN: 1");  // Игрок 1 начинает
                return;  // Завершаем обработку пустого хода
            }

            // Получаем объект игры для клиента
            KalahGame game = games[clientSocket];

            string[] parts = request.Split(',');
            int pit = int.Parse(parts[1]);  // Получаем лунку для хода

            string mode = clientModes[clientSocket];
            GameStrategy strategy;

            if (mode == "COMPUTER")
            {
                strategy = new AIPlayerGameStrategy();  // Используем стратегию для игры с компьютером
            }
            else
            {
                strategy = new NetworkGameStrategy();  // Используем стратегию для игры по сети
            }

            // Выполняем ход
            string boardState = strategy.ProcessInput(request);

            // Отправляем обновленное состояние доски
            SendMessage(clientSocket, "BOARD_STATE:" + boardState);

            // Проверяем, завершена ли игра
            if (game.IsGameOver())
            {
                // Игра завершена
                SendMessage(clientSocket, "GAME_OVER:" + boardState);
                Socket opponent = playerPairs[clientSocket];
                SendMessage(opponent, "GAME_OVER:" + boardState);
            }
            else
            {
                // Переключаем ход
                Socket opponent = playerPairs[clientSocket];
                SendMessage(opponent, "TURN: " + game.GetCurrentPlayer());
            }
        }




        // Обработка запроса на выбор режима игры
        static void HandleSetModeRequest(Socket clientSocket, string request)
        {
            string[] parts = request.Split(',');
            string mode = parts[1];

            // Устанавливаем режим игры
            if (mode == "COMPUTER")
            {
                clientModes[clientSocket] = "COMPUTER";
                SendMessage(clientSocket, "SUCCESS:MODE_SET,COMPUTER");
                Console.WriteLine($"Клиент {clientSocket.RemoteEndPoint} выбрал режим: Игра с компьютером.");
            }
            else if (mode == "PLAYER")
            {
                clientModes[clientSocket] = "PLAYER";
                SendMessage(clientSocket, "SUCCESS:MODE_SET,PLAYER");
                Console.WriteLine($"Клиент {clientSocket.RemoteEndPoint} выбрал режим: Игра с другим игроком.");
            }
            else
            {
                SendMessage(clientSocket, "ERROR: Invalid game mode.");
            }
        }


        // Обработка запроса на игру с другим игроком или с компьютером
        static void HandlePlayRequest(Socket clientSocket, string request)
        {
            lock (lockObj)
            {
                // Если клиент выбрал режим игры, создаем игру
                KalahGame game = new KalahGame();  // Создаем новую игру для клиента
                games[clientSocket] = game;  // Сохраняем игру в словарь

                if (request.Contains("COMPUTER"))
                {
                    clientModes[clientSocket] = "COMPUTER";  // Игрок выбрал игру с компьютером
                    SendMessage(clientSocket, "START_GAME_WITH_COMPUTER");

                    // Инициализируем доску
                    SendMessage(clientSocket, "BOARD:" + game.GetBoardState());
                    SendMessage(clientSocket, "TURN: 1");  // Игрок 1 начинает
                }
                else if (request.Contains("PLAYER"))
                {
                    clientModes[clientSocket] = "PLAYER";  // Игрок выбрал игру с другим игроком
                    Socket opponent = FindOpponent(clientSocket);  // Ищем соперника

                    if (opponent != null)
                    {
                        playerPairs[clientSocket] = opponent;
                        playerPairs[opponent] = clientSocket;

                        // Создаем игру для пары игроков
                        games[clientSocket] = game;
                        games[opponent] = game;

                        // Отправляем начальное состояние игры
                        SendMessage(clientSocket, "START_GAME_WITH_PLAYER");
                        SendMessage(opponent, "START_GAME_WITH_PLAYER");

                        // Отправляем начальное состояние доски
                        SendMessage(clientSocket, "BOARD:" + game.GetBoardState());
                        SendMessage(opponent, "BOARD:" + game.GetBoardState());

                        // Определяем, кто начинает первым
                        SendMessage(clientSocket, "TURN: 1");
                        SendMessage(opponent, "TURN: 2");
                    }
                    else
                    {
                        // Если нет соперника, сообщаем о необходимости подождать
                        SendMessage(clientSocket, "WAITING_FOR_PLAYER");
                    }
                }
            }
        }

        // Найти доступного соперника
        static Socket FindOpponent(Socket client)
        {
            foreach (Socket otherClient in clients)
            {
                if (otherClient != client && !playerPairs.ContainsKey(otherClient))
                {
                    return otherClient;
                }
            }
            return null;
        }

        // Отправка сообщения клиенту
        static void SendMessage(Socket client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data);
        }
    }
}