using System;
using System.Reflection;

namespace EZShare.Common.Net
{
    public class AnnouncingPackage
    {
        public string Version { get; }

        public string HostName { get; }

        public int TransportPort { get; }

        public string PublicKey { get; }

        public AnnouncingPackage(int transferPort, string publicKey)
        {
            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            HostName = Environment.MachineName;
            TransferPort = transferPort;
            PublicKey = publicKey;
        }
    }
}
