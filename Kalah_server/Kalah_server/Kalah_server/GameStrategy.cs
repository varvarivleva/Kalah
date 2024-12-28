using System;
using Kalah_server;
using System.Net.Sockets;

abstract class GameStrategy
{
    public abstract void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Dictionary<Socket, int> playerScore, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage);
    public abstract void GetTopScoreForPlayer (Socket client, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage);
}

class NetworkGameStrategy : GameStrategy
{
    private KalahBoard board = new KalahBoard();

    public override void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Dictionary<Socket, int> playerScore, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage)
    {
        if (!games.ContainsKey(client))
        {
            sendMessage(client, "ERROR: No active game. Start a new game first.");
            return;
        }

        KalahGame game = games[client];

        int currentPlayerIndex = game.GetCurrentPlayer();

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
            if (winner == 1 && currentPlayerIndex == 1 || winner == 2 && currentPlayerIndex == 2)
            {
                sendMessage(client, "GAME_OVER: Вы виграли!");
                sendMessage(opponent, "GAME_OVER: Вы проиграли");
            }
            else
            {
                sendMessage(client, "GAME_OVER: Вы проиграли");
                sendMessage(opponent, "GAME_OVER: Вы выиграли!");
            }
            

            // Убираем из словарей
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

    public override void GetTopScoreForPlayer(Socket client, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage)
    {
        var arrayScore = TopScoresPlayersDatabase.GetTopScores();
        string result = "TOP_SCORES:";
        for (int i = 0; i < arrayScore.Length; i++)
        {
            result += arrayScore[i] + ',';
        }
        sendMessage(client, result);
    }
}

class AIPlayerGameStrategy : GameStrategy
{
    private KalahBoard board = new KalahBoard();
    private Random random = new Random();

    public override void ProcessMove(Socket client, string request, Dictionary<Socket, KalahGame> games, Dictionary<Socket, Socket> playerPairs, Dictionary<Socket, int> playerScore, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage)
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
            sendMessage(client, "ERROR: Неверный ход. Попробуйте другой.");
            return;
        }

        sendMessage(client, "BOARD_STATE:" + game.GetBoardState());
        if (game.IsGameOver())
        {
            int winner = game.GetWinner();
            if (winner == 1)
            {
                sendMessage(client, $"GAME_OVER:Вы выиграли!");
            }
            else
            {
                sendMessage(client, $"GAME_OVER:Вы проиграли.");
            }
          
        }
        else
        {
                int aiPit = new Random().Next(0, 6);
                while (!game.MakeMove(aiPit))
                {
                    aiPit = new Random().Next(0, 6);
                }
                sendMessage(client, "BOARD_STATE:" + game.GetBoardState());
            
        }
    }
    public override void GetTopScoreForPlayer(Socket client, Dictionary<Socket, string> playerName, Action<Socket, string> sendMessage)
    {
        var arrayScore = TopScoresComputerDatabase.GetTopScores();
        string result = "TOP_SCORES:";
        for (int i = 0; i < arrayScore.Length; i++)
        {
            result += arrayScore[i] + ',';
        }
        sendMessage(client, result);
    }

}

