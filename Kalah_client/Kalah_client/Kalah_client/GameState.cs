namespace KalahClient
{
    public class GameState
    {
        public int[,] Pits { get; private set; } = new int[2, 6]; // Лунки (по 6 на игрока)
        public int[] Kalaha { get; private set; } = new int[2];  // Калах каждого игрока

        public GameState()
        {
            // Инициализация доски
            for (int i = 0; i < 6; i++)
            {
                Pits[0, i] = 6; // Лунки игрока 1
                Pits[1, i] = 6; // Лунки игрока 2
            }
        }

        public bool IsGameOver()
        {
            // Проверка на окончание игры
            bool player1Empty = true, player2Empty = true;
            for (int i = 0; i < 6; i++)
            {
                if (Pits[0, i] > 0) player1Empty = false;
                if (Pits[1, i] > 0) player2Empty = false;
            }
            return player1Empty || player2Empty;
        }

        public void CaptureRemainingStones(int player)
        {
            // Захват оставшихся камней
            for (int i = 0; i < 6; i++)
            {
                Kalaha[player] += Pits[player, i];
                Kalaha[1 - player] += Pits[1 - player, i];
                Pits[player, i] = 0;
                Pits[1 - player, i] = 0;
            }
        }
    }
}
