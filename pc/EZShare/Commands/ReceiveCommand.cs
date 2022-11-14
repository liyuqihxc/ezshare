using EZShare.Common.Net;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EZShare.Commands
{
    internal class ReceiveCommand : Command
    {
        private static readonly string _commandName = "receive";
        private static readonly string _commandDesc = "";

        public ReceiveCommand() : base(_commandName, _commandDesc)
        {
            var announcingPortOption = new Option<int>(name: "--Announcing-Port", getDefaultValue: () => 11456, description: "");
            announcingPortOption.AddAlias("-a");
            AddOption(announcingPortOption);

            var privateKeyFileOption = new Option<FileInfo>(name: "--Private-Key", description: "");
            privateKeyFileOption.AddAlias("-p");
            AddOption(privateKeyFileOption);

            Handler = CommandHandler.Create<bool, int, FileInfo>(CommandAction);
        }

        private async Task CommandAction(bool verbose, int announcingPort, FileInfo privateKey)
        {
            if (verbose)
                Program.LogLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;

            if (!privateKey?.Exists ?? false)
            {
                Console.WriteLine("Private key file not found.");
                return;
            }

            string pemKey;
            using (var a = privateKey!.OpenText())
                pemKey = await a.ReadToEndAsync();

            byte[] ecPublicKey, ecPrivateKey;
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportFromPem(pemKey);
                ecPrivateKey = ecdsa.ExportECPrivateKey();
                ecPublicKey = ecdsa.ExportSubjectPublicKeyInfo();
            }
            using var announcer = new Announcer(announcingPort, ecPublicKey, ecPrivateKey);
            Console.ReadKey();
        }
    }
}
