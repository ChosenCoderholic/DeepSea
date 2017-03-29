using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            
            Socket streamSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1080);
            streamSocket.Bind(localEndPoint);

            Bitmap receivedBitmap;
            int receivedBytes = 0;

            while (true)
            {
                receivedBytes = streamSocket.Receive(buffer);

                if (DeepSea.GetPacketType(buffer) == PacketType.Stream)
                {
                    //receivedBitmap = BitmapFactory.DecodeByteArray(buffer, 1, receivedBytes - 1);

                    //Bitmap mutableBitmap = receivedBitmap.Copy(Bitmap.Config.Argb8888, true);

                    //Canvas canvas = new Canvas(mutableBitmap);
                    //canvas.DrawBitmap(receivedBitmap, null, new Rect(100, 200, 200, 300), null);
                    //draw
                }
            }
            


            // Create your application here
        }
    }
}