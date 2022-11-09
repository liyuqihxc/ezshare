using Serilog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZShare.Commands
{
    internal class ToolRootCommand : RootCommand
    {
        private static readonly string _commandDesc = "";

        public ToolRootCommand() : base(_commandDesc)
        {
            var verboseOption = new Option<bool>(name: "--verbose", description: "Make the operation more talkative");
            verboseOption.AddAlias("-v");
            AddOption(verboseOption);
            Handler = CommandHandler.Create<bool>(CommandAction);
        }

        private void CommandAction(bool verbose)
        {
        }
    }
}
