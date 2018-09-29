using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.ExtensionMethods;
using Wist.Node.Core.Common;
using Wist.Node.Console.Properties;
using Wist.Node.Core.Exceptions;
using System.Collections.Generic;
using Wist.Core.Cryptography;

namespace Wist.Node.Console
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            //int minWorkerThreads, minCompletionPortThreads;
            //int maxWorkerThreads, maxCompletionPortThreads;
            //ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            //ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            //ThreadPool.SetMaxThreads(minWorkerThreads * 100, minCompletionPortThreads);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            XmlConfigurator.Configure(_log.Logger.Repository);

            _log.Info("===== NODE STARTED =====");

            ConfigureUnhandledExceptions();

            string secretKey = null;

            if ((args?.Length ?? 0) == 0)
            {
                if (Debugger.IsAttached)
                {
                    byte[] seed = CryptoHelper.GetRandomSeed();
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

            //TODO: add code for graceful shutdown
        }

        private static void ValidateProcessNotRunning()
        {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (processes.Length > 1)
            {
                System.Console.WriteLine(string.Format(Resources.INSTANCE_ALREADY_RUN, Process.GetCurrentProcess().ProcessName));

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
