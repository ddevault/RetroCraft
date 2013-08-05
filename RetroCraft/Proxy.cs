using System;
using System.Net;
using Craft.Net.Anvil;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using Craft.Net.Common;
using System.Collections.Generic;
using Craft.Net.Networking;
using ClassicStream = Craft.Net.Classic.Common.MinecraftStream;
using ClassicPacket = Craft.Net.Classic.Networking.IPacket;
using ClassicReader = Craft.Net.Classic.Networking.PacketReader;

namespace RetroCraft
{
    public class Proxy
    {
        public delegate void PacketHandler(RemoteClient client, Proxy proxy, IPacket packet);
        public delegate void ClassicHandler(RemoteClient client, Proxy proxy, ClassicPacket packet);

        public IPEndPoint LocalEndpoint { get; set; }
        public IPEndPoint RemoteEndpoint { get; set; }
        public List<RemoteClient> Clients { get; set; }
        public TcpListener Listener { get; set; }

        protected internal RSACryptoServiceProvider CryptoServiceProvider { get; set; }
        protected internal RSAParameters ServerKey { get; set; }
        protected internal object NetworkLock { get; set; }

        protected Thread NetworkThread { get; set; }
        protected PacketHandler[] PacketHandlers { get; set; }
        protected ClassicHandler[] ClassicPacketHandlers { get; set; }

        private DateTime LastPing { get; set; }

        public Proxy(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            LocalEndpoint = localEndpoint;
            RemoteEndpoint = remoteEndpoint;
            NetworkLock = new object();
            Clients = new List<RemoteClient>();
            PacketHandlers = new PacketHandler[256];
            ClassicPacketHandlers = new ClassicHandler[256];
            ModernHandlers.PacketHandlers.Register(this);
            ClassicHandlers.PacketHandlers.Register(this);
            LastPing = DateTime.MinValue;
        }

        public void Start()
        {
            CryptoServiceProvider = new RSACryptoServiceProvider(1024);
            ServerKey = CryptoServiceProvider.ExportParameters(true);

            Listener = new TcpListener(LocalEndpoint);
            Listener.Start();
            Listener.BeginAcceptTcpClient(AcceptClientAsync, null);

            NetworkThread = new Thread(NetworkWorker);
            NetworkThread.Start();
        }

        public void Connect(RemoteClient client)
        {
            client.ClassicClient = new TcpClient();
            client.ClassicClient.Connect(RemoteEndpoint);
            client.ClassicStream = new ClassicStream(new BufferedStream(client.ClassicClient.GetStream()));
            client.SendClassicPacket(new Craft.Net.Classic.Networking.HandshakePacket(ClassicReader.ProtocolVersion,
                client.Username, "", false));
            client.ClassicLoggedIn = true;
        }

        public void RegisterPacketHandler(byte packetId, PacketHandler handler)
        {
            PacketHandlers[packetId] = handler;
        }

        public void RegisterClassicPacketHandler(byte packetId, ClassicHandler handler)
        {
            ClassicPacketHandlers[packetId] = handler;
        }

        public void HandleCommand(string command, RemoteClient client)
        {
            if (command.StartsWith("//save "))
            {
                client.Level.SaveTo(command.Substring(7));
                client.SendChat(ChatColors.Blue + "[RetroCraft] Level saved.");
            }
        }

        protected void AcceptClientAsync(IAsyncResult result)
        {
            lock (NetworkLock)
            {
                if (Listener == null)
                    return; // Server shutting down
                var client = new RemoteClient(Listener.EndAcceptTcpClient(result));
                client.NetworkStream = new MinecraftStream(new BufferedStream(client.NetworkClient.GetStream()));
                Clients.Add(client);
                Listener.BeginAcceptTcpClient(AcceptClientAsync, null);
            }
        }

        private void NetworkWorker()
        {
            while (true)
            {
                lock (NetworkLock)
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        var client = Clients[i];
                        if (ModernWorker(ref i, client))
                        {
                            if (client.ClassicLoggedIn)
                                ClassicWorker(ref i, client);
                        }
                    }
                }
            }
        }

        internal void FlushClient(RemoteClient client)
        {
            while (client.PacketQueue.Count != 0)
            {
                IPacket nextPacket;
                if (client.PacketQueue.TryDequeue(out nextPacket))
                {
                    nextPacket.WritePacket(client.NetworkStream);
                    client.NetworkStream.Flush();
                }
            }
        }

        internal bool ModernWorker(ref int i, RemoteClient client)
        {
            bool disconnect = false;
            if (LastPing.AddSeconds(30) < DateTime.Now && client.ClassicLoggedIn)
            {
                client.SendPacket(new KeepAlivePacket(0));
                LastPing = DateTime.Now;
            }
            while (client.PacketQueue.Count != 0)
            {
                IPacket nextPacket;
                if (client.PacketQueue.TryDequeue(out nextPacket))
                {
                    nextPacket.WritePacket(client.NetworkStream);
                    Console.WriteLine("> {0}", nextPacket.GetType().Name);
                    client.NetworkStream.Flush();
                    if (nextPacket is DisconnectPacket)
                        disconnect = true;
                    if (nextPacket is EncryptionKeyResponsePacket)
                    {
                        client.NetworkStream = new MinecraftStream(new BufferedStream(new AesStream(client.NetworkClient.GetStream(), client.SharedKey)));
                        client.EncryptionEnabled = true;
                    }
                }
            }
            if (disconnect)
            {
                Clients.RemoveAt(i--);
                return false;
            }
            // Read packets
            var timeout = DateTime.Now.AddMilliseconds(10);
            while (client.NetworkClient.Available != 0 && DateTime.Now < timeout)
            {
                try
                {
                    var packet = PacketReader.ReadPacket(client.NetworkStream);
                    if (packet is DisconnectPacket)
                    {
                        Disconnect(client);
                        Clients.RemoveAt(i--);
                        return false;
                    }
                    HandlePacket(client, packet);
                }
                catch (SocketException)
                {
                    Disconnect(client);
                    Clients.RemoveAt(i--);
                    return false;
                }
                catch (InvalidOperationException e)
                {
                    new DisconnectPacket(e.Message).WritePacket(client.NetworkStream);
                    client.NetworkStream.Flush();
                    Disconnect(client);
                    Clients.RemoveAt(i--);
                    return false;
                }
                catch (Exception e)
                {
                    new DisconnectPacket(e.Message).WritePacket(client.NetworkStream);
                    client.NetworkStream.Flush();
                    Disconnect(client);
                    Clients.RemoveAt(i--);
                    return false;
                }
            }
            return true;
        }

        private void ClassicWorker(ref int i, RemoteClient client)
        {
            bool disconnect = false;
            while (client.ClassicQueue.Count != 0)
            {
                ClassicPacket nextPacket;
                if (client.ClassicQueue.TryDequeue(out nextPacket))
                {
                    nextPacket.WritePacket(client.ClassicStream);
                    client.ClassicStream.Flush();
                    if (nextPacket is Craft.Net.Classic.Networking.DisconnectPlayerPacket)
                        disconnect = true;
                }
            }
            if (disconnect)
            {
                Clients.RemoveAt(i--);
                return;
            }
            // Read packets
            var timeout = DateTime.Now.AddMilliseconds(10);
            while (client.ClassicClient.Available != 0 && DateTime.Now < timeout)
            {
                try
                {
                    var packet = ClassicReader.ReadPacket(client.ClassicStream);
                    Console.WriteLine("< {0}", packet.GetType().Name);
                    if (packet is Craft.Net.Classic.Networking.DisconnectPlayerPacket)
                    {
                        new DisconnectPacket(((Craft.Net.Classic.Networking.DisconnectPlayerPacket)packet).Reason)
                            .WritePacket(client.NetworkStream);
                        client.NetworkStream.Flush();
                        Disconnect(client);
                        Clients.RemoveAt(i--);
                        return;
                    }
                    HandleClassicPacket(client, packet);
                }
                catch (SocketException)
                {
                    Disconnect(client);
                    Clients.RemoveAt(i--);
                    return;
                }
                catch (InvalidOperationException e)
                {
                    new DisconnectPacket(e.Message).WritePacket(client.NetworkStream);
                    client.NetworkStream.Flush();
                    Clients.RemoveAt(i--);
                    return;
                }
                catch (Exception e)
                {
                    new DisconnectPacket(e.Message).WritePacket(client.NetworkStream);
                    client.NetworkStream.Flush();
                    Disconnect(client);
                    Clients.RemoveAt(i--);
                    return;
                }
            }
        }

        private void HandlePacket(RemoteClient client, IPacket packet)
        {
            if (PacketHandlers[packet.Id] == null)
                return;
            //throw new InvalidOperationException("No packet handler registered for 0x" + packet.Id.ToString("X2"));
            PacketHandlers[packet.Id](client, this, packet);
        }

        private void HandleClassicPacket(RemoteClient client, ClassicPacket packet)
        {
            if (ClassicPacketHandlers[packet.Id] == null)
                return;
            //throw new InvalidOperationException("No packet handler registered for 0x" + packet.Id.ToString("X2"));
            ClassicPacketHandlers[packet.Id](client, this, packet);
        }

        private void Disconnect(RemoteClient client)
        {
            try
            {
                if (client.ClassicClient.Connected)
                    client.ClassicClient.Close();
                if (client.NetworkClient.Connected)
                    client.NetworkClient.Close();
            } catch { }
        }
    }
}

