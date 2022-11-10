using EZShare.Common.Net;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
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

            Handler = CommandHandler.Create<bool, int>(CommandAction);
        }

        private void CommandAction(bool verbose, int announcingPort)
        {
            if (verbose)
                Program.LogLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;

        }
    }
}
