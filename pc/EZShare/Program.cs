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

            new Commands.ToolRootCommand().Invoke("receive");
            return;

            var privateKey = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEIK6U/6trv8I16jvktIzLA3ohU5cWiNjM20ISHxu8sE/joAoGCCqGSM49
AwEHoUQDQgAEhL1sCAyujv+0HmRs3Jreulk6MC4UsVHWLixC66/JRCt9zNmzLKQh
C9t4/hniAgXDH2g8ZJEaeIpyVwPktV1A9g==
-----END EC PRIVATE KEY-----";
            var publicKey = @"-----BEGIN PUBLIC KEY-----
MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEhL1sCAyujv+0HmRs3Jreulk6MC4U
sVHWLixC66/JRCt9zNmzLKQhC9t4/hniAgXDH2g8ZJEaeIpyVwPktV1A9g==
-----END PUBLIC KEY-----";
            var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(privateKey);
            var a = ecdsa.ExportSubjectPublicKeyInfo();
            Console.WriteLine("Hello, World!");
        }
    }
}