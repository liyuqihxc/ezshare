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
    public class Announcer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly UdpClient _socket;
        private readonly IPEndPoint _boardcastAddress;
        private readonly TcpListener _tcpListener;
        private readonly Task _announcingTask;
        private readonly Task _listeningTask;
        private readonly Task _acceptTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _privateKey;
        private bool _disposed;

        public Announcer(int port, string privateKey)
        {
            OnlineClients = new ObservableCollection<Peer>();
            _logger = Log.ForContext<Announcer>();
            _boardcastAddress = new IPEndPoint(IPAddress.Broadcast, port);
            _tcpListener = new TcpListener(IPAddress.Any, 0);

            _logger.Debug("Creating announcer on {0}", port);
            _socket = new UdpClient(port);
            _cancellationTokenSource = new CancellationTokenSource();
            _acceptTask = Task.Factory.StartNew(AcceptProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _announcingTask = Task.Factory.StartNew(AnnouncingProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _listeningTask = Task.Factory.StartNew(ListeningProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _privateKey = privateKey;
        }

        public ObservableCollection<Peer> OnlineClients { get; }

        private void ListeningProc(object param)
        {
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
                AnnouncingPackage packet = JsonSerializer.Deserialize<AnnouncingPackage>(json)!;
                if (!packet.VerifySignature())
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
            AnnouncingPackage packet;
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportECPrivateKey(Convert.FromBase64String(_privateKey), out var bytesRead);
                packet = new AnnouncingPackage(((IPEndPoint)_tcpListener.LocalEndpoint).Port, Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo()));
                var signature = ecdsa.SignData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet)), HashAlgorithmName.SHA1);
                packet.Signature = Convert.ToBase64String(signature);
            }
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var json = JsonSerializer.Serialize(packet);
                    var buffer = Encoding.UTF8.GetBytes(json);
                    _socket.Send(buffer, buffer.Length, _boardcastAddress);
                    _logger.Debug("Announce message sent at {0}", _boardcastAddress);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error occurred while broadcasting announce message.");
                }
                Thread.Sleep(1000);
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

        protected virtual void Dispose(bool disposing)
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
