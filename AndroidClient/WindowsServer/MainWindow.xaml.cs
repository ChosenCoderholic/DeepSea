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
    class CommunicationProvider : ICommunicationProvider
    {
        public Socket Socket;

        public bool SendPayload(byte[] payload)
        {
            if (Socket.Connected)
            {
                Socket.Send(payload);
                return true;
            }

            return false;
        }

        public CommunicationProvider()
        {
            Socket = null;
        }

        public CommunicationProvider(ProtocolType protocolType)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private DeepSeaServer DeepSeaServer;

        private int receivedBytes = 0;
        private byte[] buffer = new byte[20 * 1024];

        private Point ClientScreenSize = new Point(0, 0);

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.30"), 1920);

            CommunicationProvider provider = new CommunicationProvider(ProtocolType.Tcp);
            
            provider.Socket.Connect(clientEndPoint);

            DeepSeaServer = new DeepSeaServer(provider);

            if (!DeepSeaServer.Send(new ConnectionRequestPacket() {options = 0}))
            {
                provider.Socket.Close();
                return;
            }

            receivedBytes = provider.Socket.Receive(buffer);
            if (DeepSea.GetPacketType(buffer) != PacketType.ClientDefinition)
            {
                provider.Socket.Close();
                return;
            }

            ClientDefinitionPacket clientDefinitionPacket = DeepSea.GetPacket<ClientDefinitionPacket>(buffer, receivedBytes);
            ClientScreenSize.X = clientDefinitionPacket.width;
            ClientScreenSize.Y = clientDefinitionPacket.height;

            Point targetDefinition = ClientScreenSize;
            if (!DeepSeaServer.Send(new TargetDefinitionPacket() {width = Convert.ToUInt16(targetDefinition.X), height = Convert.ToUInt16(targetDefinition.Y)}))
            {
                provider.Socket.Close();
                return;
            }

            receivedBytes = provider.Socket.Receive(buffer);
            if (DeepSea.GetPacketType(buffer) != PacketType.StreamRequest)
            {
                provider.Socket.Close();
                return;
            }

            //TODO: Start streaming the video
            
        }
    }
}
