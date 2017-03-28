using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Lib.DeepSea;

namespace AndroidClient
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
}