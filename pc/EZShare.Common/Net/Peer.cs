using System;
using System.Collections.Generic;
using System.Text;

namespace EZShare.Common.Net
{
    public class Peer
    {
        public string Version { get; }

        public string HostName { get; }

        public int TransportPort { get; }

        public string PublicKey { get; }

        public string PeerFingerprint { get; }
    }
}
