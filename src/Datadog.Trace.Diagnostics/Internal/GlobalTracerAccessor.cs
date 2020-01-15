namespace Datadog.Trace.Diagnostics.Internal
{
    public class GlobalTracerAccessor : IGlobalTracerAccessor
    {
        public Tracer GetGlobalTracer()
        {
            return Tracer.Instance;
        }
    }
}
