using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            _client = new TcpClient();
            await _client.ConnectAsync("localhost", 8888);
            _stream = _client.GetStream();
            _ = ListenForServerMessages();
        }

        private async Task ListenForServerMessages()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessServerMessage(message);
            }
        }

        private void ProcessServerMessage(string message)
        {
            if (message == "LampOn")
            {
                Dispatcher.Invoke(() => LampButton.Background = Brushes.Yellow);
            }
            else if (message == "LampOff")
            {
                Dispatcher.Invoke(() => LampButton.Background = Brushes.Gray);
            }
            else if (message.StartsWith("UpdateText:"))
            {
                string newText = message.Substring("UpdateText:".Length);
                Dispatcher.Invoke(() => TextButton.Content = newText);
            }
        }

        private async void SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void LampButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("ToggleLamp");
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("GenerateRandomText");
        }
    }
}
