using System;
using System.Net.Sockets;
using Craft.Net.Common;
using System.Collections.Concurrent;
using Craft.Net.Networking;
using ClassicStream = Craft.Net.Classic.Common.MinecraftStream;
using ClassicPacket = Craft.Net.Classic.Networking.IPacket;

namespace RetroCraft
{
    public class RemoteClient
    {
        public RemoteClient(TcpClient client)
        {
            NetworkClient = client;
            PacketQueue = new ConcurrentQueue<IPacket>();
            ClassicQueue = new ConcurrentQueue<ClassicPacket>();
            ClassicLoggedIn = false;
        }

        public TcpClient NetworkClient { get; set; }
        public MinecraftStream NetworkStream { get; set; }
        public ConcurrentQueue<IPacket> PacketQueue { get; set; }

        public TcpClient ClassicClient { get; set; }
        public ClassicStream ClassicStream { get; set; }
        public ConcurrentQueue<ClassicPacket> ClassicQueue { get; set; }

        public string Username { get; set; }
        public bool EncryptionEnabled { get; protected internal set; }
        public bool ClassicLoggedIn { get; protected internal set; }

        protected internal byte[] VerificationToken { get; set; }
        protected internal byte[] SharedKey { get; set; }

        internal string AuthenticationHash { get; set; }

        public void SendPacket(IPacket packet)
        {
            PacketQueue.Enqueue(packet);
        }

        public void SendClassicPacket(ClassicPacket packet)
        {
            ClassicQueue.Enqueue(packet);
        }

        public void Disconnect(string reason)
        {
            SendPacket(new DisconnectPacket(reason));
        }

        public virtual void SendChat(string text)
        {
            text = string.Format("{{\"text\":\"{0}\"}}", text);
            SendPacket(new ChatMessagePacket(text));
        }
    }
}

