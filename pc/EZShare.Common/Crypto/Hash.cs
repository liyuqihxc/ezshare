using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EZShare.Common.Crypto
{
    public static class Hash
    {
        public static byte[] ComputeMD5(byte[] data)
        {
            using var md5 = MD5.Create();
            return md5.ComputeHash(data);
        }

        public static string ComputePublicKeyFingerprint(byte[] publicKey)
        {
            return "";
        }

        public static string ComputePublicKeyFingerprint(string publicKey) => ComputePublicKeyFingerprint(Convert.FromBase64String(publicKey));

        public static bool VerifyPeer
    }
}
