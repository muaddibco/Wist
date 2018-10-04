using CommonServiceLocator;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Wist.Core.Configuration;
using Wist.Core.Logging;

namespace Wist.Core.Aspects
{
    /// <summary>
    /// Attribute allowing automatic logging entering and exiting to and from public methods for classes decorated with this attribute
    /// </summary>
    [Serializable]
    public sealed class AutoLog : OnMethodBoundaryAspect
    {
        private string _methodName;
        private string _shortClassName;

        [NonSerialized]
        private Stopwatch _stopWatch;

        [NonSerialized]
        private ulong _totalRunTime;

        [NonSerialized]
        private ulong _totalRunCount;


        private bool _printExceptions;
        private bool _printTrace;
        private bool _methodRelevantForTimeMeasure;
        private bool _measureTime;


        private bool MeasureTime
        {
            get
            {
                return _methodRelevantForTimeMeasure && _measureTime;
            }
        }

        private Stopwatch StopWatch => _stopWatch ?? (_stopWatch = new Stopwatch());

        [NonSerialized]
        private ILogger _logger;

        [NonSerialized]
        private readonly IConfigurationService _configurationService;

        private ILogger Logger
        {
            get
            {
                try
                {
                    if (_logger == null)
                        _logger = ServiceLocator.Current.GetInstance<ILogger>();
                    return _logger;
                }
                catch (InvalidOperationException)
                {
                    // no logger, for tests
                    return null;
                }
            }
        }


        public AutoLog()
        {
            AspectPriority = (int)AspectsPriority.AutoLog;
            if (ServiceLocator.IsLocationProviderSet)
            {
                _configurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                _measureTime = _configurationService.Get<ILogConfiguration>()?.MeasureTime ?? false;
            }
        }

        public override void RuntimeInitialize(MethodBase method)
        {
            Logger.Initialize(method.DeclaringType.FullName);
            base.RuntimeInitialize(method);
        }

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            _printExceptions = method.Name != "ToString";
            _printTrace = method.IsPublic && method.Name.StartsWith("get_") == false && method.IsConstructor == false;
            _methodRelevantForTimeMeasure = method.Name.StartsWith("get_") == false;

            _methodName = method.Name;
            if (method.DeclaringType != null)
                _shortClassName = method.DeclaringType.Name;
        }

        [DebuggerStepThrough]
        public override void OnEntry(MethodExecutionArgs methodArgs)
        {
            if (MeasureTime)
            {
                StartTimer();
            }

            if (_printTrace == false)
            {
                return;
            }

            string message = $"Entering Method: {GenerateMethodString()}";

            Logger?.Debug(message);
        }


        [DebuggerStepThrough]
        public override void OnExit(MethodExecutionArgs args)
        {
            if (MeasureTime)
            {
                string timeStamp = StopTimer();

                if (!string.IsNullOrEmpty(timeStamp))
                {
                    string message = $"Heavy Method: {GenerateMethodString()}, {timeStamp}";

                    Logger?.Debug(message);
                }
            }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            string message;

            if (_printExceptions == false)
            {
                return;
            }

                message =
                    $" Exception: {GenerateShortExceptionString(args.Exception)},\r\n Method: {GenerateMethodString()},\r\n Args: {GenerateArgsString(args.Arguments.ToArray())},\r\n Object: {GenerateObjectString(args.Instance)},\r\n Full Exception: {GenerateLongExceptionString(args.Exception)}";
            Logger?.Error(message);
        }

        private string StopTimer()
        {
            StopWatch.Stop();
            ulong currentRunTime = (ulong)StopWatch.ElapsedMilliseconds;
            _totalRunCount++;
            StopWatch.Reset();

            if (currentRunTime == 0)
            {
                return string.Empty;
            }

            _totalRunTime += currentRunTime;


            string timeStamp =
                $"Time Measure: +{currentRunTime:N0}ms, All runs count: {_totalRunCount:N0} times, All runs time: {_totalRunTime:N0}ms";

            return timeStamp;
        }

        private void StartTimer()
        {
            StopWatch.Start();
        }

        private string GenerateShortExceptionString(Exception ex)
        {
            return ex == null ? string.Empty : $"{ex.GetType()}, {ex.Message}";
        }

        private string GenerateLongExceptionString(Exception ex)
        {
            return ex == null ? string.Empty : TypeVerboserManager.Verbose(ex);
        }

        private string GenerateObjectString(object obj)
        {
            return obj == null ? string.Empty : TypeVerboserManager.Verbose(obj);
        }

        [DebuggerStepThrough]
        private string GenerateMethodString()
        {
            return $"{_shortClassName}.{_methodName}";
        }

        private string GenerateArgsString(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }

                result.Append(TypeVerboserManager.Verbose(args[i]) ?? "null");
            }

            return result.ToString();
        }
    }
}
