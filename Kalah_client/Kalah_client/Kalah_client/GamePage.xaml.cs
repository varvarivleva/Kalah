using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GamePage : Page
    {
        private readonly bool _isGameWithComputer;

        public GamePage(bool isGameWithComputer)
        {
            InitializeComponent();
            _isGameWithComputer = isGameWithComputer;
        }

        private void EndGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу результатов
            NavigationService.Navigate(new ResultPage());
        }
    }
}
