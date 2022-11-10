using Serilog;
using Serilog.Core;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
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
        private readonly int _port;
        private readonly TcpListener _tcpListener;
        private readonly Task _announcingTask;
        private readonly Task _listeningTask;
        private readonly Task _acceptTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        public Announcer(int port)
        {
            OnlineClients = new ObservableCollection<Peer>();
            _logger = Log.ForContext<Announcer>();
            _port = port;
            _boardcastAddress = new IPEndPoint(IPAddress.Broadcast, port);
            _tcpListener = new TcpListener(IPAddress.Any, 0);

            _logger.Debug("Creating announcer on {0}", port);
            _socket = new UdpClient(port);
            _cancellationTokenSource = new CancellationTokenSource();
            _acceptTask = Task.Factory.StartNew(AcceptProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _announcingTask = Task.Factory.StartNew(AnnouncingProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _listeningTask = Task.Factory.StartNew(ListeningProc, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
        }

        public ObservableCollection<Peer> OnlineClients { get; }

        private void ListeningProc(object param)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                IPEndPoint? remoteEndPoint = null;
                var buffer = _socket.Receive(ref remoteEndPoint);
                var json = Encoding.UTF8.GetString(buffer);
                var packet = JsonSerializer.Deserialize<AnnouncingPackage>(json);
                _logger.Debug($"Announce message received for {remoteEndPoint}");
            }
        }

        private void AnnouncingProc(object param)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var packet = new AnnouncingPackage(((IPEndPoint)_tcpListener.LocalEndpoint).Port, "");
                    var json = JsonSerializer.Serialize(packet);
                    var buffer = Encoding.UTF8.GetBytes(json);
                    _socket.Send(buffer, buffer.Length, _boardcastAddress);
                    _logger.Debug("Announce message sent at {0}", _boardcastAddress);
                }
                catch(Exception e)
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
