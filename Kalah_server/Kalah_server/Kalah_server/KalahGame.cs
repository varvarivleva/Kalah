using System;
using System.Linq;
using System.Net.Sockets;

public class KalahGame
{
    private const int PITS_PER_PLAYER = 6;
    private const int INITIAL_STONES = 6;

    public int[] Board { get; private set; }
    public bool IsPlayerOneTurn { get; private set; }
    public Socket Player1Socket { get; set; }
    public Socket Player2Socket { get; set; }

    public KalahGame()
    {
        // Инициализация доски: 6 лунок на игрока + 2 калаха
        Board = new int[2 * PITS_PER_PLAYER + 2];
        for (int i = 0; i < PITS_PER_PLAYER * 2; i++)
        {
            Board[i] = INITIAL_STONES;
        }

        // Устанавливаем начальный ход первого игрока
        IsPlayerOneTurn = true;
    }

    public bool MakeMove(int pitIndex)
    {
        if (!IsValidMove(pitIndex))
            return false;

        int stones = Board[pitIndex];
        Board[pitIndex] = 0;
        int currentIndex = pitIndex;

        while (stones > 0)
        {
            currentIndex = (currentIndex + 1) % Board.Length;

            // Пропускаем калах соперника
            if (currentIndex == GetOpponentKalahIndex())
                continue;

            Board[currentIndex]++;
            stones--;
        }

        // Проверка, если последний камень попал в пустую лунку игрока
        if (IsPlayerSide(currentIndex) && Board[currentIndex] == 1)
        {
            int opponentIndex = GetOppositePitIndex(currentIndex);
            Board[GetCurrentPlayerKalahIndex()] += Board[opponentIndex] + Board[currentIndex];
            Board[opponentIndex] = 0;
            Board[currentIndex] = 0;
        }

        // Передача хода другому игроку
        if (currentIndex != GetCurrentPlayerKalahIndex())
        {
            IsPlayerOneTurn = !IsPlayerOneTurn;
        }
        return true;
    }

    public string GetBoardState()
    {
        return string.Join(",", Board);
    }

    public bool IsGameOver()
    {
        // Проверка, есть ли пустая сторона доски
        bool playerOneEmpty = Enumerable.Range(0, PITS_PER_PLAYER).All(i => Board[i] == 0);
        bool playerTwoEmpty = Enumerable.Range(PITS_PER_PLAYER + 1, PITS_PER_PLAYER).All(i => Board[i] == 0);

        return playerOneEmpty || playerTwoEmpty;
    }

    public string GetWinner()
    {
        if (!IsGameOver())
            return null;

        // Перемещение оставшихся камней в калах игроков
        for (int i = 0; i < PITS_PER_PLAYER; i++)
        {
            Board[PITS_PER_PLAYER] += Board[i];
            Board[2 * PITS_PER_PLAYER + 1] += Board[PITS_PER_PLAYER + 1 + i];
            Board[i] = 0;
            Board[PITS_PER_PLAYER + 1 + i] = 0;
        }

        int playerOneScore = Board[PITS_PER_PLAYER];
        int playerTwoScore = Board[2 * PITS_PER_PLAYER + 1];

        if (playerOneScore > playerTwoScore)
            return "Player 1 wins";
        else if (playerTwoScore > playerOneScore)
            return "Player 2 wins";
        else
            return "Draw";
    }

    private bool IsValidMove(int pitIndex)
    {
        if (pitIndex < 0 || pitIndex >= Board.Length)
            return false;

        if (!IsPlayerSide(pitIndex))
            return false;

        if (Board[pitIndex] == 0)
            return false;

        return true;
    }

    private bool IsPlayerSide(int index)
    {
        if (IsPlayerOneTurn)
            return index >= 0 && index < PITS_PER_PLAYER;

        return index >= PITS_PER_PLAYER + 1 && index < 2 * PITS_PER_PLAYER + 1;
    }

    private int GetCurrentPlayerKalahIndex()
    {
        return IsPlayerOneTurn ? PITS_PER_PLAYER : 2 * PITS_PER_PLAYER + 1;
    }

    private int GetOpponentKalahIndex()
    {
        return IsPlayerOneTurn ? 2 * PITS_PER_PLAYER + 1 : PITS_PER_PLAYER;
    }

    private int GetOppositePitIndex(int index)
    {
        return 2 * PITS_PER_PLAYER - index;
    }
}
