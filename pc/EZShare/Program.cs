using Serilog;
using Serilog.Core;
using System.CommandLine;
using System.Security.Cryptography;

namespace EZShare
{
    internal class Program
    {
        public static CancellationToken CancellationToken { get; private set; }
        public static LoggingLevelSwitch LogLevelSwitch { get; } = new();

        static void Main(string[] args)
        {
#if DEBUG
            LogLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
#else
            LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Error;
#endif
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LogLevelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            using var cts = new CancellationTokenSource();
            CancellationToken = cts.Token;

            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
            };

            new Commands.ToolRootCommand().Invoke("receive -p alice.pem");
            return;

            var privateKeyAlice = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEINiwj5PQVYOKfAxdumGCq7lg3asYrUrvbIerNPcS7EbBoAoGCCqGSM49
AwEHoUQDQgAEKLyt69y1JAof/aJuQGC7VaJ39cXW776D6WmP2/WwsmO5HTpP+ekX
dDVUp8fGdl0w5C5PdE06ZM0m83hJNihcRQ==
-----END EC PRIVATE KEY-----";
            var privateKeyBob = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEIPcrw+is1BiN0tL3bncwNaC4hXeFC9VzGVa8msR8DVYXoAoGCCqGSM49
AwEHoUQDQgAEfQdXx2b3kHGeWSl94C0MOa3p0V/Hxa81ZxN+q8amEPwnp/PLiIMB
7jSuEPrrlaT/6/Q8Slhob22C6wTRRMqiiA==
-----END EC PRIVATE KEY-----";
            using var alice = new ECDiffieHellmanCng(ECCurve.NamedCurves.nistP256);
            alice.ImportFromPem(privateKeyAlice);
            alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            alice.HashAlgorithm = CngAlgorithm.Sha256;
            var alicePublicKey = alice.PublicKey.ToByteArray();
            using var bob = new ECDiffieHellmanCng(ECCurve.NamedCurves.nistP256);
            //bob.ImportFromPem(privateKeyBob);
            bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            bob.HashAlgorithm = CngAlgorithm.Sha256;
            var bobPublicKey = bob.PublicKey.ToByteArray();
            byte[] bobKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
            byte[] aliceKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            var equal = Enumerable.SequenceEqual(bobKey, aliceKey);
        }
    }
}