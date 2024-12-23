using System.Windows;
using System.Windows.Navigation;

namespace KalahClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainFrame.Navigate(new LoginPage());
        }
    }
}
