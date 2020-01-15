namespace Datadog.Trace.Diagnostics.AspNetCore
{
    public class AspNetCoreDiagnosticOptions
    {
        public HostingOptions Hosting { get; } = new HostingOptions();
    }
}
