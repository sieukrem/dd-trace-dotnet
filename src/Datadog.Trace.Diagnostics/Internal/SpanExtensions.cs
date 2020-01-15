using System;
using Datadog.Trace.Abstractions;

namespace Datadog.Trace.Diagnostics.Internal
{
    internal static class SpanExtensions
    {
        /// <summary>
        /// Sets the error tags and adds information about the <paramref name="exception"/>
        /// to the given <paramref name="span"/>.
        /// </summary>
        public static void SetException(this ISpan span, Exception exception)
        {
            if (span != null && exception != null)
            {
                span.SetException(exception);
            }
        }
    }
}
