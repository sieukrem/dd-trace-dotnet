using System;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.Diagnostics.Internal;
using Microsoft.Extensions.Hosting;

namespace Datadog.Trace.Diagnostics
{
    /// <summary>
    /// Starts and stops all OpenTracing instrumentation components.
    /// </summary>
    internal class InstrumentationService : IHostedService
    {
        private readonly DiagnosticManager _diagnosticsManager;

        public InstrumentationService(DiagnosticManager diagnosticManager)
        {
            _diagnosticsManager = diagnosticManager ?? throw new ArgumentNullException(nameof(diagnosticManager));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _diagnosticsManager.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _diagnosticsManager.Stop();

            return Task.CompletedTask;
        }
    }
}
