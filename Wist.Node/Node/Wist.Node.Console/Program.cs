using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wist.Node.Core;
using Wist.Node.Console.Properties;

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

            NodeBootstrapper nodeBootstrapper = new NodeBootstrapper(cancellationTokenSource.Token);
            nodeBootstrapper.Run();

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
