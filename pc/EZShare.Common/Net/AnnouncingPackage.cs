using System;
using System.Reflection;
using System.Security.Cryptography;
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

        public AnnouncingPackage(int transportPort, string publicKey)
        {
            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            HostName = Environment.MachineName;
            TransportPort = transportPort;
            PublicKey = publicKey;
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

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out var bytesRead);
            var json = JsonSerializer.Serialize(new AnnouncingPackage(this));
            return ecdsa.VerifyData(Encoding.UTF8.GetBytes(json), Convert.FromBase64String(Signature), HashAlgorithmName.SHA1);
        }
    }
}
