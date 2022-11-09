using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZShare.Commands
{
    internal class GenerateKeyCommand : Command
    {
        private static readonly string _commandName = "genkey";
        private static readonly string _commandDesc = "";

        public GenerateKeyCommand() : base(_commandName, _commandDesc)
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
        }
    }
}
