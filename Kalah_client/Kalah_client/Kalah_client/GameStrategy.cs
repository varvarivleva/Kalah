using System;

namespace KalahClient
{
    public interface IGameStrategy
    {
        int MakeMove(GameState state, int player, int selectedPit); // Возвращает номер последней лунки
    }

    public class NetworkStrategy : IGameStrategy
    {
        public int MakeMove(GameState state, int player, int selectedPit)
        {
            // Логика для получения данных от другого игрока через сеть
            return selectedPit; // Реализация зависит от взаимодействия с сервером
        }
    }

    public class ComputerStrategy : IGameStrategy
    {
        public int MakeMove(GameState state, int player, int selectedPit)
        {
            // Простейший выбор хода для компьютера
            Random random = new Random();
            int validPit;
            do
            {
                validPit = random.Next(0, 6); // Компьютер выбирает лунку случайно
            } while (state.Pits[player, validPit] == 0); // Лунка должна содержать камни
            return validPit;
        }
    }
}
