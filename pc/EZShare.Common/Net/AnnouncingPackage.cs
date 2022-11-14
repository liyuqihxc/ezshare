using EZShare.Common.Crypto;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EZShare.Common.Net
{
    public class AnnouncingPackage
    {
        [JsonIgnore]
        public string Fingerprint => Crypto.Hash.ComputePublicKeyFingerprint(PublicKey);

        public string HostName { get; private set; }

        public string PublicKey { get; private set; }

        public string? Signature { get; set; }

        public int TransportPort { get; private set; }

        public string Version { get; private set; }

        public AnnouncingPackage(int transportPort, byte[] ecPublicKey)
        {
            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            HostName = Environment.MachineName;
            TransportPort = transportPort;
            PublicKey = Convert.ToBase64String(ecPublicKey);
        }

        [JsonConstructor]
        public AnnouncingPackage(string hostName, string publicKey, string signature, int transportPort, string version)
        {
            Version = version;
            HostName = hostName;
            TransportPort = transportPort;
            PublicKey = publicKey;
            Signature = signature;
        }

        private AnnouncingPackage(AnnouncingPackage package)
        {
            Version = package.Version;
            HostName = package.HostName;
            PublicKey = package.PublicKey;
            TransportPort= package.TransportPort;
        }

        public bool VerifySignature()
        {
            if (Signature == null)
                throw new InvalidOperationException();

            var json = JsonSerializer.Serialize(new AnnouncingPackage(this));
            return Hash.VerifySignature(Encoding.UTF8.GetBytes(json), Convert.FromBase64String(Signature), Convert.FromBase64String(PublicKey));
        }
    }
}
