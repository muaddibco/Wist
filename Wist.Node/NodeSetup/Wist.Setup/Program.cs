using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wist.Setup
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            XmlConfigurator.Configure();

            _log.Info("===== SETUP STARTED =====");

            ConfigureUnhandledExceptions();

            SetupBootstrapper setupBootstrapper = new SetupBootstrapper(cancellationTokenSource.Token);
            setupBootstrapper.Run();

            cancellationTokenSource.Cancel();
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
