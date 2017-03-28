using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Lib.DeepSea;

namespace AndroidClient
{
    [Activity(Label = "AndroidClient", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Landscape)]

    public class MainActivity : Activity
    {
        enum State
        {
            Initializing,
            WaitingForHost,
            WaitingForConnectionRequest,
            SendingClientDefinition,
            WaitingForTargetDefinition,
            SendingStreamRequest,
            WaitingForStream
        }
        
        private State currentState = State.Initializing;

        private Thread workerThread = null;

        private TextView txtStatus = null;

        private Point ScreenSize = new Point(0, 0);

        private DeepSeaClient DeepSeaClient = null;

        private int receivedBytes = 0;
        private byte[] buffer = new byte[20 * 1024];

        private Point TargetScreenSize = new Point(0, 0);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            txtStatus = FindViewById<TextView>(Resource.Id.txtState);

            workerThread = new Thread(() =>
            {
                CommunicationProvider provider = new CommunicationProvider(ProtocolType.Tcp);

                DeepSeaClient = new DeepSeaClient(provider);

                while (currentState != State.WaitingForStream)
                {
                    currentState = HandleConnection(ref provider.Socket);
                }


                StartActivity(typeof(StreamActivity));
                //TODO: Start stream activity
            });

            workerThread.IsBackground = true;
            workerThread.Start();

        }

        void UpdateStatusText(string text)
        {
            Action action = delegate { txtStatus.Text = text; };
            txtStatus.Post(action);
        }

        State HandleConnection(ref Socket serverSocket)
        {
            switch (currentState)
            {
                case State.Initializing:
                    UpdateStatusText("Initializing");

                    IWindowManager windowManager = GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                    windowManager.DefaultDisplay.GetRealSize(ScreenSize);

                    return State.WaitingForHost;

                case State.WaitingForHost:
                    UpdateStatusText("Waiting for host");

                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1920);
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(ipEndPoint);
                    serverSocket.Listen(100);

                    serverSocket = serverSocket.Accept();
                    return State.WaitingForConnectionRequest;

                case State.WaitingForConnectionRequest:
                    UpdateStatusText("Waiting for connection request");

                    receivedBytes = serverSocket.Receive(buffer);
                    if(DeepSea.GetPacketType(buffer) == PacketType.ConnectionRequest)
                        return State.SendingClientDefinition;

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.SendingClientDefinition:
                    UpdateStatusText("Sending client information");

                    if (DeepSeaClient.Send(new ClientDefinitionPacket() {width = Convert.ToUInt16(ScreenSize.X), height = Convert.ToUInt16(ScreenSize.Y)}))
                        return State.WaitingForTargetDefinition;

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.WaitingForTargetDefinition:
                    UpdateStatusText("Waiting for target definition");

                    receivedBytes = serverSocket.Receive(buffer);
                    if (DeepSea.GetPacketType(buffer) == PacketType.TargetDefinition)
                    {
                        TargetDefinitionPacket targetDefinitionPacket = DeepSea.GetPacket<TargetDefinitionPacket>(buffer, receivedBytes);

                        TargetScreenSize.X = targetDefinitionPacket.width;
                        TargetScreenSize.Y = targetDefinitionPacket.height;

                        return State.SendingStreamRequest;
                    }

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.SendingStreamRequest:
                    UpdateStatusText("Sending stream request");

                    if(DeepSeaClient.Send(new StreamRequestPacket() {options = 0}))
                        return State.WaitingForStream;

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.WaitingForStream:
                    UpdateStatusText("Waiting for stream");

                    receivedBytes = serverSocket.Receive(buffer);
                    if (DeepSea.GetPacketType(buffer) == PacketType.Stream)
                    {
                        return State.WaitingForStream;
                    }

                    serverSocket.Close();
                    return State.WaitingForHost;

                default:
                    return State.WaitingForHost;
            }
        }
    }
}

