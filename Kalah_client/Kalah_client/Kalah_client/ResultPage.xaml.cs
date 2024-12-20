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
        }

        private void MainMenuButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new GameModePage(_client));
        }
    }
}
