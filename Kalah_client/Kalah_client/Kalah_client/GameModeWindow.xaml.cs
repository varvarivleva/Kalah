using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace KalahClient
{
    public partial class GameModeWindow : Window
    {
        private Socket _socket;

        public GameModeWindow(Socket socket)
        {
            InitializeComponent();
            _socket = socket;
        }

        private void PlayWithPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            _socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"play_with_player\"}"));
            new MainWindow(_socket).Show();
            Close();
        }

        private void PlayWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            _socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"play_with_computer\"}"));
            new MainWindow(_socket).Show();
            Close();
        }
    }
}
