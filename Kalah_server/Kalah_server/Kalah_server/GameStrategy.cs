﻿using System;
using Kalah_server;
using System.Net.Sockets;

abstract class GameStrategy
{
    public abstract string ProcessInput(string input);
    public abstract void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Action<Socket, string> sendMessage);
}

class NetworkGameStrategy : GameStrategy
{
    private KalahBoard board = new KalahBoard();

    public override string ProcessInput(string input)
    {
        string[] parts = input.Split(',');
        int player = int.Parse(parts[0]);
        int pit = int.Parse(parts[1]);

        board.MakeMove(player, pit);
        return board.GetBoardState();
    }

    public override void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Action<Socket, string> sendMessage)
    {
        if (!games.ContainsKey(client))
        {
            sendMessage(client, "ERROR: No active game. Start a new game first.");
            return;
        }

        KalahGame game = games[client];

        int currentPlayerIndex = game.GetCurrentPlayer();
        //if (game.GetCurrentPlayer() != currentPlayerIndex)
        //{
        //    sendMessage(client, "ERROR: Not your turn.");
        //    return;
        //}

        string[] parts = request.Split(',');
        if (!int.TryParse(parts[1], out int pit) || !game.MakeMove(pit))
        {
            sendMessage(client, "ERROR: Invalid move.");
            return;
        }

        // Отправляем состояние доски обоим игрокам
        Socket opponent = playerPairs[client];
        int opponentIndex;
        // Отправляем состояние доски обоим игрокам
        if (currentPlayerIndex == 1)
            opponentIndex = 2;
        else
            opponentIndex = 1;

        sendMessage(client, "BOARD_STATE:" + game.GetBoardStateForPlayer(currentPlayerIndex));
        sendMessage(opponent, "BOARD_STATE:" + game.GetBoardStateForPlayer(opponentIndex));

        if (game.IsGameOver())
        {
            int winner = game.GetWinner();
            sendMessage(client, $"GAME_OVER: Winner is Player {winner}");
            sendMessage(opponent, $"GAME_OVER: Winner is Player {winner}");

            // Убираем из словарей
            games.Remove(client);
            games.Remove(opponent);
            playerPairs.Remove(client);
            playerPairs.Remove(opponent);
        }
        else
        {
            // Меняем очередь
            if (game.GetCurrentPlayer() == currentPlayerIndex)
            {
                sendMessage(client, "YOUR_TURN");
                sendMessage(opponent, "WAIT_TURN");
            }
            else
            {
                sendMessage(client, "WAIT_TURN");
                sendMessage(opponent, "YOUR_TURN");
            }
        }
    }


    // Получение индекса игрока (1 или 2)
    private int GetPlayerIndex(Socket client, Dictionary<Socket, Socket> playerPairs)
    {
        return playerPairs.ContainsKey(client) ? 1 : 2;
    }
}

class AIPlayerGameStrategy : GameStrategy
{
    private KalahBoard board = new KalahBoard();
    private Random random = new Random();

    public override string ProcessInput(string input)
    {
        // Игрок 1 (пользовательский) ход
        string[] parts = input.Split(',');
        int player = 1;
        int pit = int.Parse(parts[1]);

        board.MakeMove(player, pit);

        if (!board.IsGameOver())
        {
            // Ход компьютера
            int aiPit;
            do
            {
                aiPit = random.Next(0, 6);
            } while (!board.CanMove(2, aiPit));

            board.MakeMove(2, aiPit);
        }

        return board.GetBoardState();
    }
    public override void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Action<Socket, string> sendMessage)
    {
        if (!games.ContainsKey(client))
        {
            sendMessage(client, "ERROR: No active game. Start a new game first.");
            return;
        }

        KalahGame game = games[client];
        string[] parts = request.Split(',');
        if (!int.TryParse(parts[1], out int pit) || !game.MakeMove(pit))
        {
            sendMessage(client, "ERROR: Invalid move.");
            return;
        }

        sendMessage(client, "BOARD_STATE:" + game.GetBoardState());
        if (game.IsGameOver())
        {
            int winner = game.GetWinner();
            sendMessage(client, $"GAME_OVER:Winner is Player {winner}");
            games.Remove(client);
        }
        else
        {
            // Логика для компьютера
            if (!game.IsGameOver())
            {
                int aiPit = new Random().Next(0, 6);
                while (!game.MakeMove(aiPit))
                {
                    aiPit = new Random().Next(0, 6);
                }
                sendMessage(client, "BOARD_STATE:" + game.GetBoardState());
            }
        }
    }

}

