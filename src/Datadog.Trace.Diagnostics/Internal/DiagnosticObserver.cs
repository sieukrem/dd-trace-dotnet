using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Datadog.Trace.Diagnostics.Internal
{
    internal abstract class DiagnosticObserver
    {
        protected ILogger Logger { get; }

        protected IDatadogTracer Tracer { get; }

        protected bool IsLogLevelTraceEnabled { get; }

        protected DiagnosticObserver(ILoggerFactory loggerFactory, IDatadogTracer tracer)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            Logger = loggerFactory.CreateLogger(GetType());
            Tracer = tracer;

            IsLogLevelTraceEnabled = Logger.IsEnabled(LogLevel.Trace);
        }

        public virtual bool IsSubscriberEnabled()
        {
            return true;
        }

        public abstract IDisposable SubscribeIfMatch(DiagnosticListener diagnosticListener);
    }
}
