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
            if (_state.Pits[_currentPlayer, selectedPit] == 0) return; // ������ ������ �� ������ �����

            int stones = _state.Pits[_currentPlayer, selectedPit];
            _state.Pits[_currentPlayer, selectedPit] = 0;

            int index = selectedPit;
            int player = _currentPlayer;

            while (stones > 0)
            {
                index++;
                if (index > 5)
                {
                    if (player == _currentPlayer) // ���� ��� ��� �����
                    {
                        _state.Kalaha[player]++;
                        stones--;
                        if (stones == 0) return; // ���������� ����
                    }
                    player = 1 - player; // ������ ������
                    index = 0;
                }
                else
                {
                    _state.Pits[player, index]++;
                    stones--;
                }
            }

            // ������
            if (player == _currentPlayer && index < 6 && _state.Pits[player, index] == 1)
            {
                int opponentIndex = 5 - index;
                _state.Kalaha[player] += _state.Pits[1 - player, opponentIndex];
                _state.Pits[1 - player, opponentIndex] = 0;
            }

            // �������� �� ���������� ����
            if (_state.IsGameOver())
            {
                _state.CaptureRemainingStones(_currentPlayer);
            }
            else
            {
                _currentPlayer = 1 - _currentPlayer; // �������� ����
            }
        }

        public string GetBoardState()
        {
            // ����������� ��������� ����� ��� �����������
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"����� ������ 1: {_state.Kalaha[0]}");
            sb.Append("����� ������ 1: ");
            for (int i = 0; i < 6; i++) sb.Append($"{_state.Pits[0, i]} ");
            sb.AppendLine();
            sb.Append("����� ������ 2: ");
            for (int i = 0; i < 6; i++) sb.Append($"{_state.Pits[1, i]} ");
            sb.AppendLine();
            sb.AppendLine($"����� ������ 2: {_state.Kalaha[1]}");
            return sb.ToString();
        }
    }

}
