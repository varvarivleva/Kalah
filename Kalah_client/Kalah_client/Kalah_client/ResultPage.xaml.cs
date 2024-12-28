using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class ResultPage : Page
    {
        private readonly TcpKalahClient _client;

        public ResultPage(TcpKalahClient client)
        {
            InitializeComponent();
            _client = client;
            _client.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("SCORE"))
            {
                HandleScore(message);
            }
            else if (message.StartsWith("TOP_SCORES"))
            {
                HandleTopScore(message);
            }
            else if (message.StartsWith("ERROR"))
            {
                MessageBox.Show("Ошибка: " + message.Substring(6));
            }
        }

        private void HandleScore(string message)
        {
            string[] parts = message.Split(':');
            Dispatcher.Invoke(() =>
            {
                PlayerScore.Text = $"Ваш результат: {parts[1]}";
            });
            _client.SendMessage("TOP_SCORES");
        }

        private void HandleTopScore(string message)
        {
            string[] topScores = message.Substring("TOP_SCORES:".Length).Split(',');
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < topScores.Length-1; i+=2)
                {
                    TopScores.Text += $"{topScores[i]} \t {topScores[i+1]}\n";
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _client.ClearMessageReceivedHandlers();
            NavigationService.Navigate(new GameModePage(_client));
        }
    }
}
