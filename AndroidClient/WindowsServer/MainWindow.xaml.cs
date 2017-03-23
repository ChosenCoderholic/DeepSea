using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Lib.DeepSea;

namespace WindowsServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        byte[] data = new byte[3 * 1024];

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.34"), 1920);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            clientSocket.Connect(clientEndPoint);
            
            int size = clientSocket.Receive(data);

            ClientDefinitionPacket packet = DeepSea.GetPacket<ClientDefinitionPacket>(data, size);

            MessageBox.Show($"Daten: typ={PacketType.ClientDefinition}; x={ packet.width }; y={packet.height}");
        }
    }
}
