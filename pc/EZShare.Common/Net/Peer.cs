using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EZShare.Common.Net
{
    public class Peer
    {
        public Peer(string version, string hostName, string publicKey, IPEndPoint peerEndPoint)
        {
            Version = version;
            HostName = hostName;
            PeerEndPoint = peerEndPoint;
            PublicKey = Convert.FromBase64String(publicKey);
        }

        public string Version { get; set; }

        public string HostName { get; set; }

        public IPEndPoint PeerEndPoint { get; }

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
    }
}
