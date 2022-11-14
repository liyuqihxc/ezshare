using EZShare.Common.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EZShare.Common.Crypto
{
    public static class Hash
    {
        public static byte[] ComputeSHA1(byte[] data)
        {
            using var sha1 = SHA1.Create();
            return sha1.ComputeHash(data);
        }

        public static string ComputePublicKeyFingerprint(byte[] publicKey)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(publicKey);
            return string.Join(':', hash.Select(b => b.ToString("x2")));
        }

        public static string ComputePublicKeyFingerprint(string publicKey) => ComputePublicKeyFingerprint(Convert.FromBase64String(publicKey));

        public static byte[] SignData(byte[] data, byte[] ecPrivateKey)
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(ecPrivateKey, out var bytesRead);
            return ecdsa.SignData(data, HashAlgorithmName.SHA1);
        }

        public static bool VerifySignature(byte[] data, byte[] signature, byte[] ecPublicKey)
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(ecPublicKey, out var bytesRead);
            return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA1);
        }

        public static bool VerifyPeer()
        {
            throw new NotImplementedException();
        }
    }
}
