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
    class PacketSender : IPacketSender
    {
        public static Socket socket = null;

        public bool SendPayload(byte[] payload)
        {
            if (socket.Connected)
            {
                socket.Send(payload);
                return true;
            }

            return false;
        }
    }


    [Activity(Label = "AndroidClient", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Landscape)]

    public class MainActivity : Activity
    {

        //int count = 1;

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


        private DeepSeaClient deepSeaClient;
        private IWindowManager windowManager;
        private Point ScreenSize = new Point(0, 0);
        private State currentState = State.Initializing;
        private Thread workerThread = null;

        private TextView txtStatus = null;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            txtStatus = FindViewById<TextView>(Resource.Id.txtState);

            workerThread = new Thread(() =>
            {
                PacketSender.socket = null;

                while (currentState != State.WaitingForStream)
                {
                    currentState = HandleConnection(ref PacketSender.socket);
                }
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
                    windowManager = GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
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
                    //TODO: Wait for packet
                    return State.SendingClientDefinition;

                case State.SendingClientDefinition:
                    UpdateStatusText("Sending client information");
                    if (SendClientInfo(serverSocket))
                        return State.WaitingForTargetDefinition;

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.WaitingForTargetDefinition:
                    UpdateStatusText("Waiting for target definition");
                    //TODO: Wait for packet
                    return State.SendingStreamRequest;

                case State.SendingStreamRequest:
                    UpdateStatusText("Sending stream request");
                    return State.WaitingForStream;

                case State.WaitingForStream:
                    UpdateStatusText("Waiting for stream");

                    return State.WaitingForStream;

                default:
                    return State.WaitingForHost;
            }
        }

        private bool SendClientInfo(Socket serverSocket)
        {
            //byte[] width = BitConverter.GetBytes(Convert.ToUInt16(ScreenSize.X));
            //byte[] height = BitConverter.GetBytes(Convert.ToUInt16(ScreenSize.Y));

            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(width);
            //    Array.Reverse(height);
            //}

            //byte[] tcpPayload = new[]
            //{
            //    Convert.ToByte(PacketType.ClientDefinition),
            //    width[0],
            //    width[1],
            //    height[0],
            //    height[1]
            //};

            //width = null;
            //height = null;

            //if (serverSocket.Connected)
            //{
            //    serverSocket.Send(tcpPayload);
            //    return true;
            //}

            deepSeaClient.Send(new ClientDefinitionPacket()
            {
                width = Convert.ToUInt16(ScreenSize.X),
                height = Convert.ToUInt16(ScreenSize.Y)
            });

            return false;
        }
    }
}

