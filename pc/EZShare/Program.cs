using Serilog;

namespace EZShare
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
#if DEBUG
                .Debug()
#else
                .Information()
#endif
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Console.WriteLine("Hello, World!");
        }
    }
}