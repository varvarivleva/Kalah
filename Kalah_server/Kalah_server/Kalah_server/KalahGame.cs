using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalah_server
{
    public class KalahGame
    {
        private KalahBoard board;
        private int currentPlayer;

        public KalahGame()
        {
            board = new KalahBoard();
            currentPlayer = 1; // Игрок 1 ходит первым
        }

        // Метод для выполнения хода
        public bool MakeMove(int pitIndex)
        {
            if (!board.CanMove(currentPlayer, pitIndex))
            {
                return false; // Невозможный ход (пустая лунка или неправильный индекс)
            }

            // Сделать ход
            board.MakeMove(currentPlayer, pitIndex);

            // Проверка на окончание игры
            if (board.IsGameOver())
            {
                return true; // Игра завершена
            }

            // Меняем игрока
            currentPlayer = (currentPlayer == 1) ? 2 : 1;

            return false; // Ход завершен, игра продолжается
        }

        // Получение текущего состояния доски для отправки клиентам
        public string GetBoardState()
        {
            return board.GetBoardState();
        }

        // Проверка на завершение игры
        public bool IsGameOver()
        {
            return board.IsGameOver();
        }

        // Проверка, чей сейчас ход
        public int GetCurrentPlayer()
        {
            return currentPlayer;
        }
    }


}
