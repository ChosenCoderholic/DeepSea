using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
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

            currentState = State.Initializing;

            currentState = State.WaitingForHost;

            Socket serverSocket = null;

            while (currentState != State.WaitingForStream)
            {
                currentState = HandleConnection(serverSocket);
            }

        }

        State HandleConnection(Socket serverSocket)
        {
            switch (currentState)
            {
                case State.WaitingForHost:
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1080);
                    

                    serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Raw, ProtocolType.Tcp);
                    
                    serverSocket.Accept();
                    

                    return State.SendingClientInfo;

                case State.WaitingForConnectionRequest:
                    //TODO: Wait for packet
                    return State.SendingClientInfo;

                case State.SendingClientInfo:
                    SendClientInfo(serverSocket);
                    return State.WaitingForTargetDefinition;

                case State.WaitingForStream:

                    return State.WaitingForStream;

                default:
                    return State.WaitingForHost;
            }
        }

        private void SendClientInfo(Socket serverSocket)
        {
            DisplayMetrics displayMetrics = new DisplayMetrics();

            byte[] tcpPayload = new[]
            {
                Convert.ToByte(PacketType.ClientDefinition),
                Convert.ToByte(displayMetrics.WidthPixels),
                Convert.ToByte(displayMetrics.HeightPixels)
            };

            displayMetrics = null;

            serverSocket.Send(tcpPayload);
        }
    }
}

