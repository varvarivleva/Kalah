using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

class KalahServer
{
    private static List<Socket> clients = new List<Socket>();
    private static Dictionary<Socket, Socket> playerPairs = new Dictionary<Socket, Socket>();
    private static Dictionary<Socket, KalahGame> games = new Dictionary<Socket, KalahGame>();
    private static object lockObj = new object();

    static void Main()
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.200.13"), 4563));
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

                try
                {
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
                    else if (request.Action == "play_with_player")
                    {
                        HandleMultiplayerRequest(clientSocket);
                    }
                    else if (request.Action == "play_with_computer")
                    {
                        StartGameWithAI(clientSocket);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка десериализации JSON: {ex.Message}");
                    Console.WriteLine($"Входящие данные: {receivedData}");
                    SendMessage(clientSocket, "ERROR:Invalid JSON format");
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

    static void HandleMultiplayerRequest(Socket clientSocket)
    {
        lock (lockObj)
        {
            Socket opponent = FindOpponent(clientSocket);

            if (opponent != null)
            {
                playerPairs[clientSocket] = opponent;
                playerPairs[opponent] = clientSocket;

                var game = new KalahGame();
                games[clientSocket] = game;
                games[opponent] = game;

                SendMessage(clientSocket, "START_GAME_WITH_PLAYER");
                SendMessage(opponent, "START_GAME_WITH_PLAYER");
            }
            else
            {
                SendMessage(clientSocket, "WAITING_FOR_PLAYER");
            }
        }
    }

    static void StartGameWithAI(Socket clientSocket)
    {
        lock (lockObj)
        {
            var game = new KalahGame();
            games[clientSocket] = game;

            SendMessage(clientSocket, "START_GAME_WITH_COMPUTER");
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
