using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace KalahClient
{
    public partial class LoginWindow : Window
    {
        private Socket _socket;

        public LoginWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect("192.168.1.101", 4563);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;
            string message = $"{{\"action\":\"login\",\"username\":\"{username}\",\"password\":\"{password}\"}}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            _socket.Send(data);

            byte[] buffer = new byte[1024];
            int bytesRead = _socket.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (response == "OK")
            {
                new GameModeWindow(_socket).Show();
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка авторизации.");
            }
        }
    }
}
