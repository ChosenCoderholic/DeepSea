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
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.58"), 1920);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            clientSocket.Connect(clientEndPoint);
            //clientSocket.Send(Encoding.ASCII.GetBytes("DasIstEineTestNachricht"));
            
            int size = clientSocket.Receive(data);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data, 1, 2);
                Array.Reverse(data, 3, 2);
            }

            ushort screenWidth = BitConverter.ToUInt16(data, 1);
            ushort screenHeight = BitConverter.ToUInt16(data, 3);

            MessageBox.Show($"Daten: typ={data[0].ToString()}; x={ BitConverter.ToUInt16(data, 1).ToString() }; y={BitConverter.ToUInt16(data, 3).ToString()}");
        }
    }
}
