using System.Text;

namespace KalahClient
{
    public class KalahGame
    {
        private GameState _state = new GameState();
        private IGameStrategy _strategy;
        private int _currentPlayer = 0;

        public KalahGame(IGameStrategy strategy)
        {
            _strategy = strategy;
        }
        public GameState State => _state;

        public void MakeMove(int selectedPit)
        {
            if (_state.Pits[_currentPlayer, selectedPit] == 0) return; // Нельзя ходить из пустой лунки

            int stones = _state.Pits[_currentPlayer, selectedPit];
            _state.Pits[_currentPlayer, selectedPit] = 0;

            int index = selectedPit;
            int player = _currentPlayer;

            while (stones > 0)
            {
                index++;
                if (index > 5)
                {
                    if (player == _currentPlayer) // Если это наш калах
                    {
                        _state.Kalaha[player]++;
                        stones--;
                        if (stones == 0) return; // Завершение хода
                    }
                    player = 1 - player; // Меняем игрока
                    index = 0;
                }
                else
                {
                    _state.Pits[player, index]++;
                    stones--;
                }
            }

            // Захват
            if (player == _currentPlayer && index < 6 && _state.Pits[player, index] == 1)
            {
                int opponentIndex = 5 - index;
                _state.Kalaha[player] += _state.Pits[1 - player, opponentIndex];
                _state.Pits[1 - player, opponentIndex] = 0;
            }

            // Проверка на завершение игры
            if (_state.IsGameOver())
            {
                _state.CaptureRemainingStones(_currentPlayer);
            }
            else
            {
                _currentPlayer = 1 - _currentPlayer; // Передача хода
            }
        }

        public string GetBoardState()
        {
            // Форматируем состояние доски для отображения
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Калах Игрока 1: {_state.Kalaha[0]}");
            sb.Append("Лунки Игрока 1: ");
            for (int i = 0; i < 6; i++) sb.Append($"{_state.Pits[0, i]} ");
            sb.AppendLine();
            sb.Append("Лунки Игрока 2: ");
            for (int i = 0; i < 6; i++) sb.Append($"{_state.Pits[1, i]} ");
            sb.AppendLine();
            sb.AppendLine($"Калах Игрока 2: {_state.Kalaha[1]}");
            return sb.ToString();
        }
    }

}
