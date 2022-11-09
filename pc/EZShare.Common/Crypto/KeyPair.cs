using System;
using System.Collections.Generic;
using System.Text;

namespace EZShare.Common.Crypto
{
    public class KeyPair
    {
        public string PublicKey { get; }

        public string PrivateKey { get; }

        public string PublicKeyFingerprint { get; }
    }
}
