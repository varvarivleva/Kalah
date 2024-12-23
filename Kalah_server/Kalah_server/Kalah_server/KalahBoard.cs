using System;

class KalahBoard
{
    private int[] board = new int[14];
    private bool extraTurn = false;

    public KalahBoard()
    {
        for (int i = 0; i < 6; i++)
        {
            board[i] = board[i + 7] = 6;
        }
    }

    public void MakeMove(int player, int pit)
    {
        int index = player == 1 ? pit : pit + 7;
        int stones = board[index];
        board[index] = 0;

        while (stones > 0)
        {
            index = (index + 1) % 14;

            if (index == 6 && player == 2) continue; // Skip opponent's Kalah
            if (index == 13 && player == 1) continue; // Skip opponent's Kalah

            board[index]++;
            stones--;
        }

        // Check extra turn or capture logic
        if (index == (player == 1 ? 6 : 13))
        {
            extraTurn = true;
        }
        else if (index < 6 && player == 1 && board[index] == 1)
        {
            board[6] += board[index + 7] + 1;
            board[index] = board[index + 7] = 0;
        }
        else if (index > 6 && index < 13 && player == 2 && board[index] == 1)
        {
            board[13] += board[index - 7] + 1;
            board[index] = board[index - 7] = 0;
        }
    }

    public string GetBoardState()
    {
        return string.Join(",", board);
    }

    public bool IsGameOver()
    {
        return Array.TrueForAll(board[0..6], x => x == 0) || Array.TrueForAll(board[7..13], x => x == 0);
    }

    public bool CanMove(int player, int pit)
    {
        int index = player == 1 ? pit : pit + 7;
        return board[index] > 0;
    }
    public int GetWinner()
    {
        // Собираем оставшиеся камни, если игра закончилась
        if (IsGameOver())
        {
            for (int i = 0; i < 6; i++)
            {
                board[6] += board[i];
                board[i] = 0;

                board[13] += board[i + 7];
                board[i + 7] = 0;
            }
        }

        // Сравниваем количество камней в "калахах"
        if (board[6] > board[13]) return 1;
        if (board[6] < board[13]) return 2;
        return 0; // Ничья
    }

}
