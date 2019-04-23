using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.Cryptography;
using Wist.Node.Core.Common;
using Wist.Node.Core.Exceptions;
using Wist.Core.ExtensionMethods;
using Wist.Node.Properties;
using Wist.Crypto.ConfidentialAssets;

namespace Wist.Node
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            XmlConfigurator.Configure(_log.Logger.Repository);

            _log.Info("===== NODE STARTED =====");

            ConfigureUnhandledExceptions();

            string secretKey = null;

            if ((args?.Length ?? 0) == 0)
            {
                if (Debugger.IsAttached)
                {
                    byte[] seed = ConfidentialAssetsHelper.GetRandomSeed();
                    secretKey = seed.ToHexString();
                }
                else
                {
                    throw new NoSecretKeyProvidedException();
                }
            }
            else
            {
                secretKey = args[0];
            }

            Dictionary<string, string> bootArgs = new Dictionary<string, string>
            {
                { "secretKey", secretKey }
            };

            NodeBootstrapper nodeBootstrapper = new NodeBootstrapper(cancellationTokenSource.Token);
            nodeBootstrapper.Run(bootArgs);

            string command = null;
            do
            {
                command = System.Console.ReadLine();
            } while (command?.ToLower() != "exit");

            cancellationTokenSource.Cancel();

            //TODO: add code for gracefull shutdown
        }

        private static void ValidateProcessNotRunning()
        {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (processes.Length > 1)
            {
                Console.WriteLine(string.Format(Resources.INSTANCE_ALREADY_RUN, Process.GetCurrentProcess().ProcessName));

                Environment.Exit(0);
            }
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
