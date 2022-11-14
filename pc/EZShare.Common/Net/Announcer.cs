using EZShare.Common.Crypto;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EZShare.Common.Net
{
    public sealed class Announcer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly UdpClient _socket;
        private readonly int _announcePort;
        private readonly TcpListener _tcpListener;
        private readonly Task _announcingTask;
        private readonly Task _listeningTask;
        private readonly Task _acceptTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly byte[] _ecPublicKey;
        private readonly byte[] _ecPrivateKey;
        private bool _disposed;

        public Announcer(int announcePort, byte[] ecPublicKey, byte[] ecPrivateKey)
        {
            OnlineClients = new ObservableCollection<Peer>();
            _logger = Log.ForContext<Announcer>();
            _announcePort = announcePort;
            _tcpListener = new TcpListener(IPAddress.Any, 0);

            _logger.Debug("Creating announcer on {0}", announcePort);
            _socket = new UdpClient(new IPEndPoint(IPAddress.Any, announcePort));
            _cancellationTokenSource = new CancellationTokenSource();
            _acceptTask = Task.Factory.StartNew(AcceptProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _announcingTask = Task.Factory.StartNew(AnnouncingProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _listeningTask = Task.Factory.StartNew(ListeningProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _ecPublicKey = ecPublicKey;
            _ecPrivateKey = ecPrivateKey;
        }

        public int AnnounceInterval { get; set; } = 1000;

        public ObservableCollection<Peer> OnlineClients { get; }

        private void ListeningProc(object param)
        {
            var fingerprint = Hash.ComputePublicKeyFingerprint(_ecPublicKey);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_socket.Available == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                IPEndPoint? remoteEndPoint = null;
                var buffer = _socket.Receive(ref remoteEndPoint);
                var json = Encoding.UTF8.GetString(buffer);
                var packet = JsonSerializer.Deserialize<AnnouncingPackage>(json)!;
                if (packet.Fingerprint == fingerprint || !packet.VerifySignature())
                    continue;
                var peerEndPoint = new IPEndPoint(remoteEndPoint.Address, packet.TransportPort);
                if (!OnlineClients.Any(c => c.PeerFingerprint == packet.Fingerprint))
                {
                    OnlineClients.Add(new Peer(packet.Version, packet.HostName, packet.PublicKey, peerEndPoint));
                }
                else
                {
                    var peer = OnlineClients.First(c => c.PeerFingerprint == packet.Fingerprint);
                    if (peer.PeerEndPoint != peerEndPoint)
                    {
                        OnlineClients.Remove(peer);
                        OnlineClients.Add(new Peer(packet.Version, packet.HostName, packet.PublicKey, peerEndPoint));
                    }
                    else
                    {
                        peer.Version = packet.Version;
                        peer.HostName = packet.HostName;
                    }
                }
                _logger.Debug($"Announce message received from {remoteEndPoint}");
            }
        }

        private void AnnouncingProc(object param)
        {
            AnnouncingPackage packet = new AnnouncingPackage(((IPEndPoint)_tcpListener.LocalEndpoint).Port, _ecPublicKey);
            packet.Signature = Convert.ToBase64String(
                Hash.SignData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet)), _ecPrivateKey)
            );
            IPEndPoint boardcastAddress = new IPEndPoint(IPAddress.Broadcast, _announcePort);
            var json = JsonSerializer.Serialize(packet);
            var buffer = Encoding.UTF8.GetBytes(json);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    _socket.Send(buffer, buffer.Length, boardcastAddress);
                    _logger.Debug("Announce message sent at {0}", boardcastAddress);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error occurred while broadcasting announce message.");
                }
                Thread.Sleep(AnnounceInterval);
            }
        }

        private void AcceptProc(object param)
        {
            _tcpListener.Start();
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var newClient = _tcpListener.AcceptTcpClient();
                    var receiver = new FileReceiver(newClient);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error occurred while accepting incoming connection.");
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _announcingTask.Wait();
                    _listeningTask.Wait();
                    _cancellationTokenSource.Dispose();
                    _socket.Close();
                    _socket.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
