using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EZShare.Common.Net
{
    public class Peer
    {
        private TcpClient? _tcpClient;

        internal Peer(string version, string hostName, byte[] ecPublicKey, IPEndPoint? peerEndPoint)
        {
            Version = version;
            HostName = hostName;
            PeerEndPoint = peerEndPoint;
            PublicKey = ecPublicKey;
        }

        internal Peer(TcpClient tcpClient, Action<Peer> callBack)
        {
            _tcpClient = tcpClient;
            new Thread(PrepareForReceivingFiles) { IsBackground = true }.Start();
        }

        public string Version { get; set; }

        public string HostName { get; set; }

        public IPEndPoint? PeerEndPoint { get; }

        public byte[] PublicKey { get; }

        private string? _peerFingerprint;
        public string PeerFingerprint
        {
            get
            {
                if (_peerFingerprint == null)
                {
                    _peerFingerprint = Crypto.Hash.ComputePublicKeyFingerprint(PublicKey);
                }
                return _peerFingerprint;
            }
        }

        private void PrepareForReceivingFiles()
        {

        }
    }

    public interface IPeerEventHandler
    {
        void OnConnectionEstablished();
    }

    public interface IFileIncomingHandler : IPeerEventHandler
    {

    }
}
