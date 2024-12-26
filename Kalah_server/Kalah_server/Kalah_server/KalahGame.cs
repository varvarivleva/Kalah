using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            int index = currentPlayer == 1 ? pitIndex : pitIndex + 7;

            if (!board.CanMove(currentPlayer, index))
            {
                return false; // Невозможный ход (пустая лунка или неправильный индекс)
            }

            // Сделать ход
            board.MakeMove(currentPlayer, pitIndex);

            // Меняем игрока
            currentPlayer = (currentPlayer == 1) ? 2 : 1;

            return true; // Ход завершен, игра продолжается
        }

        // Получение текущего состояния доски для отправки клиентам
        public string GetBoardState()
        {
            return board.GetBoardState();
        }

        public string GetBoardStateForPlayer(int player)
        {
            int[] boardState = board.GetRawBoard();
            if (player == 1)
            {
                return string.Join(",", boardState); // Игрок 1 видит доску как есть
            }
            else
            {
                // Для игрока 2 переворачиваем доску
                int[] player2View = new int[14];
                Array.Copy(boardState, 7, player2View, 0, 7); // Копируем 7-13
                Array.Copy(boardState, 0, player2View, 7, 7); // Копируем 0-6
                return string.Join(",", player2View);
            }
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

        public int GetWinner()
        {
            return board.GetWinner();
        }

    }


}
