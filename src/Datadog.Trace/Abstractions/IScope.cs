using System;

namespace Datadog.Trace.Abstractions
{
    public interface IScope : IDisposable
    {
        ISpan Span { get; }
    }
}
