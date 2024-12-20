using System.Windows.Controls;

namespace KalahClient
{
    public partial class ResultPage : Page
    {
        public ResultPage()
        {
            InitializeComponent();
        }

        private void MainMenuButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Возврат в главное меню
            NavigationService.Navigate(new GameModePage());
        }
    }
}
