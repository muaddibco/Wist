using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wist.Setup.Simulation
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure(_log.Logger.Repository);

            _log.Info("===== SIMULATION SETUP STARTED =====");

            ConfigureUnhandledExceptions();

            SetupBootstrapper bootstrapper = new SetupBootstrapper(CancellationToken.None);
            bootstrapper.ResetDatabase = args?.Contains("--WipeAll") ?? false;
            bootstrapper.Run();

            Console.WriteLine("Press <ENTER> for exit");
            Console.ReadLine();

            _log.Info("===== SIMULATION SETUP COMPLETED =====");
        }

        private static void ConfigureUnhandledExceptions()
        {
            if (Process.GetCurrentProcess().ProcessName.EndsWith(".vshost")) return;

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                _log.Error("Unhandled exception caught", args.Exception);
                args.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    _log.Error("Unhandled exception caught", ex);
                }
                else
                {
                    _log.Error(args.ExceptionObject?.ToString());
                }
            };
        }
    }
}
