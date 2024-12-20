using System;
using System.Linq;

public class KalahGame
{
    private const int PITS_PER_PLAYER = 6;
    private const int INITIAL_STONES = 6;

    private int[] board; // Массив для хранения состояния лунок и калахов
    private bool isPlayerOneTurn;

    public KalahGame()
    {
        // Инициализация доски: 6 лунок на игрока + 2 калаха
        board = new int[2 * PITS_PER_PLAYER + 2];
        for (int i = 0; i < PITS_PER_PLAYER * 2; i++)
        {
            board[i] = INITIAL_STONES;
        }

        // Устанавливаем начальный ход первого игрока
        isPlayerOneTurn = true;
    }

    public int[] GetBoard() => board.ToArray();

    public bool IsPlayerOneTurn => isPlayerOneTurn;

    public bool MakeMove(int pitIndex)
    {
        if (!IsValidMove(pitIndex))
            return false;

        int stones = board[pitIndex];
        board[pitIndex] = 0;
        int currentIndex = pitIndex;

        while (stones > 0)
        {
            currentIndex = (currentIndex + 1) % board.Length;

            // Пропускаем калах соперника
            if (currentIndex == GetOpponentKalahIndex())
                continue;

            board[currentIndex]++;
            stones--;
        }

        // Проверка, если последний камень попал в пустую лунку игрока
        if (IsPlayerSide(currentIndex) && board[currentIndex] == 1)
        {
            int opponentIndex = GetOppositePitIndex(currentIndex);
            board[GetCurrentPlayerKalahIndex()] += board[opponentIndex] + board[currentIndex];
            board[opponentIndex] = 0;
            board[currentIndex] = 0;
        }

        // Проверка на дополнительный ход
        if (currentIndex == GetCurrentPlayerKalahIndex())
        {
            return true; // Игроку предоставляется дополнительный ход
        }

        // Передача хода другому игроку
        isPlayerOneTurn = !isPlayerOneTurn;
        return true;
    }

    public bool IsGameOver()
    {
        // Проверка, есть ли пустая сторона доски
        bool playerOneEmpty = Enumerable.Range(0, PITS_PER_PLAYER).All(i => board[i] == 0);
        bool playerTwoEmpty = Enumerable.Range(PITS_PER_PLAYER + 1, PITS_PER_PLAYER).All(i => board[i] == 0);

        return playerOneEmpty || playerTwoEmpty;
    }

    public string GetWinner()
    {
        if (!IsGameOver())
            return null;

        // Перемещение оставшихся камней в калах игроков
        for (int i = 0; i < PITS_PER_PLAYER; i++)
        {
            board[PITS_PER_PLAYER] += board[i];
            board[2 * PITS_PER_PLAYER + 1] += board[PITS_PER_PLAYER + 1 + i];
            board[i] = 0;
            board[PITS_PER_PLAYER + 1 + i] = 0;
        }

        int playerOneScore = board[PITS_PER_PLAYER];
        int playerTwoScore = board[2 * PITS_PER_PLAYER + 1];

        if (playerOneScore > playerTwoScore)
            return "Player 1";
        else if (playerTwoScore > playerOneScore)
            return "Player 2";
        else
            return "Draw";
    }

    private bool IsValidMove(int pitIndex)
    {
        if (pitIndex < 0 || pitIndex >= board.Length)
            return false;

        if (!IsPlayerSide(pitIndex))
            return false;

        if (board[pitIndex] == 0)
            return false;

        return true;
    }

    private bool IsPlayerSide(int index)
    {
        if (isPlayerOneTurn)
            return index >= 0 && index < PITS_PER_PLAYER;

        return index >= PITS_PER_PLAYER + 1 && index < 2 * PITS_PER_PLAYER + 1;
    }

    private int GetCurrentPlayerKalahIndex()
    {
        return isPlayerOneTurn ? PITS_PER_PLAYER : 2 * PITS_PER_PLAYER + 1;
    }

    private int GetOpponentKalahIndex()
    {
        return isPlayerOneTurn ? 2 * PITS_PER_PLAYER + 1 : PITS_PER_PLAYER;
    }

    private int GetOppositePitIndex(int index)
    {
        return 2 * PITS_PER_PLAYER - index;
    }
}
