﻿// Серверная часть с поддержкой нескольких игроков и логикой игры "Калах"
using System;
using System.Collections.Generic;
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

                // Обработка авторизации
                if (receivedData.Contains("\"action\":\"login\""))
                {
                    var requestData = JsonSerializer.Deserialize<LoginRequest>(receivedData);
                    bool isAuthorized = Database.Authorize(requestData.Username, requestData.Password);

                    string response = isAuthorized ? "OK" : "FAIL";
                    SendMessage(clientSocket, response);

                    if (isAuthorized)
                    {
                        // Сохраняем пользователя в файл БД
                        File.AppendAllText("users.txt", $"{requestData.Username},{requestData.Password},{0}\n");
                    }
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


    static void HandleMove(Socket clientSocket, string receivedData)
    {
        string moveData = receivedData.Replace("MOVE:", "");
        int pitIndex = int.Parse(moveData);
        KalahGame game = games[clientSocket];
        bool isPlayer1 = IsPlayer1(clientSocket, game);

        lock (lockObj)
        {
            if (game.MakeMove(isPlayer1, pitIndex))
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
            // После того как клиент делает ход, ИИ тоже делает свой ход.
            // Логика для ИИ (например, случайный ход):
            Thread aiThread = new Thread(() => PlayAI(clientSocket));
            aiThread.Start();
        }
    }

    // Пример реализации хода ИИ (вы можете улучшить логику ИИ)
    static void PlayAI(Socket clientSocket)
    {
        // Здесь реализуется логика ИИ. Например, просто выбираем случайный ход.
        KalahGame game = games[clientSocket];
        Random random = new Random();

        // Ищем возможные ходы для ИИ (находим пустые ячейки)
        List<int> availableMoves = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (game.Pits[1, i] > 0) // ИИ играет за второго игрока
            {
                availableMoves.Add(i);
            }
        }

        if (availableMoves.Count > 0)
        {
            // ИИ делает случайный ход
            int move = availableMoves[random.Next(availableMoves.Count)];

            // Делаем ход для ИИ
            game.MakeMove(false, move); // false - ИИ это второй игрок

            // Отправляем состояние доски клиенту
            string boardState = game.GetBoardState();
            SendMessage(clientSocket, "BOARD:" + boardState);

            // Проверка на конец игры
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

// Класс KalahGame для логики игры "Калах"
class KalahGame
{
    public int[,] Pits { get; private set; } // 2 строки, 6 лунок для каждого игрока
    public int[] Kalaha { get; private set; } // 2 Калаха
    public Socket Player1Socket { get; set; }

    private bool player1Turn;

    public KalahGame()
    {
        Pits = new int[2, 6];
        Kalaha = new int[2];
        for (int i = 0; i < 6; i++)
        {
            Pits[0, i] = 6;
            Pits[1, i] = 6;
        }
        player1Turn = new Random().Next(0, 2) == 0;
    }

    public bool MakeMove(bool isPlayer1, int pitIndex)
    {
        int playerIndex = isPlayer1 ? 0 : 1;

        if (Pits[playerIndex, pitIndex] == 0) return false; // Пустая лунка

        int stones = Pits[playerIndex, pitIndex];
        Pits[playerIndex, pitIndex] = 0;
        int currentSide = playerIndex;
        int currentIndex = pitIndex;

        while (stones > 0)
        {
            currentIndex++;

            if (currentSide == 0 && currentIndex == 6) // Калах игрока 1
            {
                Kalaha[0]++;
                stones--;
                if (stones == 0 && player1Turn) return true; // Дополнительный ход
                currentSide = 1;
                currentIndex = -1;
            }
            else if (currentSide == 1 && currentIndex == 6) // Калах игрока 2
            {
                Kalaha[1]++;
                stones--;
                if (stones == 0 && !player1Turn) return true; // Дополнительный ход
                currentSide = 0;
                currentIndex = -1;
            }
            else
            {
                Pits[currentSide, currentIndex]++;
                stones--;
            }
        }

        player1Turn = !player1Turn;
        return true;
    }

    public string GetBoardState()
    {
        return string.Join(",", Pits.Cast<int>()) + $":{Kalaha[0]}:{Kalaha[1]}";
    }

    public bool IsGameOver()
    {
        bool player1Empty = true;
        bool player2Empty = true;

        for (int i = 0; i < 6; i++)
        {
            if (Pits[0, i] > 0) player1Empty = false;
            if (Pits[1, i] > 0) player2Empty = false;
        }

        return player1Empty || player2Empty;
    }

    public string GetWinner()
    {
        if (IsGameOver())
        {
            for (int i = 0; i < 6; i++)
            {
                Kalaha[0] += Pits[0, i];
                Kalaha[1] += Pits[1, i];
                Pits[0, i] = 0;
                Pits[1, i] = 0;
            }

            if (Kalaha[0] > Kalaha[1]) return "PLAYER_1_WINS";
            if (Kalaha[1] > Kalaha[0]) return "PLAYER_2_WINS";
            return "DRAW";
        }

        return "";
    }
}
