using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

class KalahServerя
{
    private static List<Socket> clients = new List<Socket>();
    private static Dictionary<Socket, Socket> playerPairs = new Dictionary<Socket, Socket>();
    private static Dictionary<Socket, KalahGame> games = new Dictionary<Socket, KalahGame>();
    private static object lockObj = new object();

    static void Main()
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 4563)); // Измените на нужный IP адрес
        serverSocket.Listen(10);
        Console.WriteLine("Сервер запущен, ожидает подключения...");

        try
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("Новый клиент подключен.");
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
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
            clients.Add(clientSocket);
        }

        try
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = clientSocket.Receive(buffer);
                if (bytesRead == 0) break;

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Получено от клиента: {receivedData}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Игнорируем регистр ключей JSON
                };

                // Попытка десериализовать данные
                var request = JsonSerializer.Deserialize<Request>(receivedData, options);

                if (request.Action == "login")
                {
                    HandleLoginRequest(clientSocket, request);
                }
                else if (request.Action == "register")
                {
                    HandleRegisterRequest(clientSocket, request);
                }


                // Обработка игры с другим игроком
                else if (receivedData.Contains("\"action\":\"play_with_player\""))
                {
                    HandleMultiplayerRequest(clientSocket);
                }
                // Обработка игры с компьютером
                else if (receivedData.Contains("\"action\":\"play_with_computer\""))
                {
                    StartGameWithAI(clientSocket);
                }
                else if (receivedData.StartsWith("MOVE:") && games.ContainsKey(clientSocket))
                {
                    HandleMove(clientSocket, receivedData);
                }
                else if (receivedData.Equals("END_TURN") && games.ContainsKey(clientSocket))
                {
                    HandleEndTurn(clientSocket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(clientSocket);
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

    static void HandleLoginRequest(Socket clientSocket, Request request)
    {
        bool isAuthorized = Database.Authorize(request.Username, request.Password);

        var authData = JsonSerializer.Serialize(new
        {
            action = "login",
            message = isAuthorized ? "OK" : "FAIL"
        });

        SendMessage(clientSocket, authData);
    }

    static void HandleRegisterRequest(Socket clientSocket, Request request)
    {
        string _message;

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
        {
            _message = "FAIL:Invalid registration data";
        }

        else if (Database.Authorize(request.Username, request.Password))
        {
            _message = "FAIL:User already exists";
        }

        else
        {
            Database.AddUser(request.Username, request.Password);
            _message = "OK:Registration successful";
        }

        var registerData = JsonSerializer.Serialize(new
        {
            action = "register",
            message = _message
        });

        SendMessage(clientSocket, registerData);
    }

    static void HandleMove(Socket clientSocket, string receivedData)
    {
        string moveData = receivedData.Replace("MOVE:", "");
        int pitIndex = int.Parse(moveData);
        KalahGame game = games[clientSocket];
        bool isPlayer1 = IsPlayer1(clientSocket, game);

        lock (lockObj)
        {
            if (game.MakeMove(pitIndex))
            {
                string boardState = game.GetBoardState();
                SendMessage(clientSocket, "BOARD:" + boardState);

                if (playerPairs.ContainsKey(clientSocket))
                {
                    Socket opponent = playerPairs[clientSocket];
                    SendMessage(opponent, "BOARD:" + boardState);
                }

                if (game.IsGameOver())
                {
                    string result = game.GetWinner();
                    SendMessage(clientSocket, "GAME_OVER:" + result);
                    if (playerPairs.ContainsKey(clientSocket))
                    {
                        Socket opponent = playerPairs[clientSocket];
                        SendMessage(opponent, "GAME_OVER:" + result);
                    }
                }
            }
            else
            {
                SendMessage(clientSocket, "INVALID_MOVE");
            }
        }
    }

    static void HandleEndTurn(Socket clientSocket)
    {
        KalahGame game = games[clientSocket];
        // Отправка сигнала о завершении хода
        string boardState = game.GetBoardState();
        SendMessage(clientSocket, "BOARD:" + boardState); // Отправляем текущее состояние доски игроку

        if (playerPairs.ContainsKey(clientSocket))
        {
            Socket opponent = playerPairs[clientSocket];
            SendMessage(opponent, "BOARD:" + boardState); // Отправляем состояние доски сопернику
        }
    }

    static void HandleMultiplayerRequest(Socket clientSocket)
    {
        lock (lockObj)
        {
            // Ищем доступного соперника
            Socket opponent = FindOpponent(clientSocket);

            if (opponent != null)
            {
                // Если соперник найден, связываем игроков
                playerPairs[clientSocket] = opponent;
                playerPairs[opponent] = clientSocket;

                // Создаем новую игру
                var game = new KalahGame();
                games[clientSocket] = game;
                games[opponent] = game;

                // Отправляем обоим игрокам сообщение о старте игры
                SendMessage(clientSocket, "START_GAME_WITH_PLAYER");
                SendMessage(opponent, "START_GAME_WITH_PLAYER");
            }
            else
            {
                // Если соперник не найден, отправляем сообщение о том, что клиент должен подождать
                SendMessage(clientSocket, "WAITING_FOR_PLAYER");
            }
        }
    }

    static void StartGameWithAI(Socket clientSocket)
    {
        lock (lockObj)
        {
            // Создаем игру с ИИ
            var game = new KalahGame();
            games[clientSocket] = game;

            // Отправляем клиенту сообщение о начале игры с ИИ
            SendMessage(clientSocket, "START_GAME_WITH_COMPUTER");

            // Пример логики для хода ИИ (это можно улучшить):
            Thread aiThread = new Thread(() => PlayAI(clientSocket));
            aiThread.Start();
        }
    }

    static void PlayAI(Socket clientSocket)
    {
        // Логика ИИ (например, случайный ход)
        KalahGame game = games[clientSocket];
        Random random = new Random();

        List<int> availableMoves = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (game.Board[1 + i] > 0) // ИИ ходит из лунок игрока 2
            {
                availableMoves.Add(1 + i);
            }
        }

        if (availableMoves.Count > 0)
        {
            int move = availableMoves[random.Next(availableMoves.Count)];
            game.MakeMove(move);

            string boardState = game.GetBoardState();
            SendMessage(clientSocket, "BOARD:" + boardState);

            if (game.IsGameOver())
            {
                string result = game.GetWinner();
                SendMessage(clientSocket, "GAME_OVER:" + result);
            }
        }
    }

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

    static bool IsPlayer1(Socket client, KalahGame game)
    {
        return games.ContainsKey(client) && games[client].Player1Socket == client;
    }

    static void SendMessage(Socket client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.Send(data);
    }
}

public class Request
{
    public string Action { get; set; } // Действие: "login", "register", "play_with_player", etc.
    public string Username { get; set; } // Логин пользователя
    public string Password { get; set; } // Пароль пользователя
    public string Email { get; set; } // Email (для регистрации)
}
