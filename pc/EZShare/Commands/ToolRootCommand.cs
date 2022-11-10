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

        public Option<bool> VerboseOption { get; }

        public ToolRootCommand() : base(_commandDesc)
        {
            AddCommand(new ReceiveCommand());
            AddCommand(new GenerateKeyCommand());

            VerboseOption = new Option<bool>(name: "--verbose", description: "Make the operation more talkative");
            VerboseOption.AddAlias("-v");
            AddGlobalOption(VerboseOption);
        }
    }
}
