using System.Windows;
using System.Windows.Controls;

namespace KalahClient
{
    public partial class GameModePage : Page
    {
        public GameModePage()
        {
            InitializeComponent();
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика сетевой игры
            NavigationService.Navigate(new GamePage(false));
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика игры с компьютером
            NavigationService.Navigate(new GamePage(true));
        }
    }
}
