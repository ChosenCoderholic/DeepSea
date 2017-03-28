using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;
using Lib.DeepSea;

namespace AndroidClient
{
    [Activity(Label = "StreamActivity", ScreenOrientation = ScreenOrientation.Landscape)]
    public class StreamActivity : Activity
    {
        byte[] buffer = new byte[20 * 1024 * 1024];
        private Canvas screenBackbuffer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Stream);
            
            CommunicationProvider communicationProvider = new CommunicationProvider(ProtocolType.Udp);
            DeepSeaClient deepSeaClient = new DeepSeaClient(communicationProvider);
            

            while (true)
            {
                communicationProvider.Socket.Receive(buffer);

                if (DeepSea.GetPacketType(buffer) == PacketType.Stream)
                {
                    
                }
            }
            


            // Create your application here
        }
    }
}