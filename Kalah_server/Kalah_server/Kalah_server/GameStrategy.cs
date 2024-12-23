using System;

abstract class GameStrategy
{
    public abstract string ProcessInput(string input);
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
}

