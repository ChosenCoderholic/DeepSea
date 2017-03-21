using System;
using System.Net;
using System.Net.Sockets;
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

namespace AndroidClient
{
    
    [Activity(Label = "AndroidClient", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Landscape)]
    
    public class MainActivity : Activity
    {

        //int count = 1;

        enum State
        {
            Initializing,
            WaitingForHost,
            WaitingForConnectionRequest,
            SendingClientInfo,
            WaitingForTargetDefinition,
            SendingStreamRequest,
            WaitingForStream
        }

        enum PacketType
        {
            //Init
            ConnectionRequest = 1, 
            ClientDefinition = 2,

            //Definitions
            TargetDefinition = 3,
            ReadyForStream = 4,

            //Stream
            Stream = 0,
        }

        IWindowManager windowManager;
        private Point ScreenSize = new Point(0, 0);
        State currentState = State.Initializing;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //// Get our button from the layout resource,
            //// and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.MyButton);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            
            Socket serverSocket = null;

            currentState = State.Initializing;

            while (currentState != State.WaitingForStream)
            {
                currentState = HandleConnection(ref serverSocket);
            }

        }

        State HandleConnection(ref Socket serverSocket)
        {
            switch (currentState)
            {
                case State.Initializing:
                    windowManager = GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                    windowManager.DefaultDisplay.GetRealSize(ScreenSize);
                    return State.WaitingForHost;

                case State.WaitingForHost:
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1920);
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(ipEndPoint);
                    serverSocket.Listen(100);
                    
                    serverSocket = serverSocket.Accept();

                    return State.WaitingForConnectionRequest;

                case State.WaitingForConnectionRequest:
                    //TODO: Wait for packet
                    return State.SendingClientInfo;

                case State.SendingClientInfo:
                    if(SendClientInfo(serverSocket))
                        return State.WaitingForTargetDefinition;

                    serverSocket.Close();
                    return State.WaitingForHost;

                case State.WaitingForTargetDefinition:
                //TODO: Wait for packet

                case State.SendingStreamRequest:

                case State.WaitingForStream:

                    return State.WaitingForStream;

                default:
                    return State.WaitingForHost;
            }
        }

        private bool SendClientInfo(Socket serverSocket)
        {
            byte[] width = BitConverter.GetBytes(Convert.ToUInt16(ScreenSize.X));
            byte[] height = BitConverter.GetBytes(Convert.ToUInt16(ScreenSize.Y));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(width);
                Array.Reverse(height);
            }
    
            byte[] tcpPayload = new[]
            {
                Convert.ToByte(PacketType.ClientDefinition),
                width[0],
                width[1],
                height[0],
                height[1]
            };

            width = null;
            height = null;

            if (serverSocket.Connected)
            {
                serverSocket.Send(tcpPayload);
                return true;
            }

            return false;
        }
    }
}

