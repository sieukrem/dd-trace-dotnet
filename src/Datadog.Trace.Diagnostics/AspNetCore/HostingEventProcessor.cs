using System;
using Datadog.Trace.Abstractions;
using Datadog.Trace.Diagnostics.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Datadog.Trace.Diagnostics.AspNetCore
{
    internal class HostingEventProcessor
    {
        private static readonly PropertyFetcher _httpRequestIn_start_HttpContextFetcher = new PropertyFetcher("HttpContext");
        private static readonly PropertyFetcher _httpRequestIn_stop_HttpContextFetcher = new PropertyFetcher("HttpContext");
        private static readonly PropertyFetcher _unhandledException_HttpContextFetcher = new PropertyFetcher("httpContext");
        private static readonly PropertyFetcher _unhandledException_ExceptionFetcher = new PropertyFetcher("exception");

        internal static readonly string NoHostSpecified = String.Empty;

        private readonly IDatadogTracer _tracer;
        private readonly ILogger _logger;
        private readonly HostingOptions _options;

        public HostingEventProcessor(IDatadogTracer tracer, ILogger logger, HostingOptions options)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public bool ProcessEvent(string eventName, object arg)
        {
            switch (eventName)
            {
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start":
                    {
                        var httpContext = (HttpContext)_httpRequestIn_start_HttpContextFetcher.Fetch(arg);

                        if (ShouldIgnore(httpContext))
                        {
                            _logger.LogDebug("Ignoring request");
                        }
                        else
                        {
                            var request = httpContext.Request;

                            ISpanContext extractedSpanContext = null;

                            if (_options.ExtractEnabled?.Invoke(httpContext) ?? true)
                            {
                                // TODO: lucas
                                // extractedSpanContext = _tracer.Extract(BuiltinFormats.HttpHeaders, new RequestHeadersExtractAdapter(request.Headers));
                            }

                            string operationName = _options.OperationNameResolver(httpContext);

                            Span span = _tracer.StartSpan(operationName, extractedSpanContext)
                                               .SetTag(Tags.InstrumentationName, _options.ComponentName)
                                               .SetTag(Tags.SpanKind, SpanKinds.Server)
                                               .SetTag(Tags.HttpMethod, request.Method)
                                               .SetTag(Tags.HttpUrl, GetDisplayUrl(request));

                            Scope scope = _tracer.ActivateSpan(span);

                            _options.OnRequest?.Invoke(scope.Span, httpContext);
                        }
                    }
                    return true;

                case "Microsoft.AspNetCore.Hosting.UnhandledException":
                    {
                        ISpan span = _tracer.ScopeManager.Active?.Span;

                        if (span != null)
                        {
                            var exception = (Exception)_unhandledException_ExceptionFetcher.Fetch(arg);
                            var httpContext = (HttpContext)_unhandledException_HttpContextFetcher.Fetch(arg);

                            span.SetException(exception);
                            _options.OnError?.Invoke(span, exception, httpContext);
                        }
                    }
                    return true;

                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop":
                    {
                        IScope scope = _tracer.ScopeManager.Active;

                        if (scope != null)
                        {
                            var httpContext = (HttpContext)_httpRequestIn_stop_HttpContextFetcher.Fetch(arg);
                            scope.Span.SetTag(Tags.HttpStatusCode, httpContext.Response.StatusCode.ToString());
                            scope.Dispose();
                        }
                    }
                    return true;

                default: return false;
            }
        }

        private static string GetDisplayUrl(HttpRequest request)
        {
            if (request.Host.HasValue)
            {
                return request.GetDisplayUrl();
            }

            // HTTP 1.0 requests are not required to provide a Host to be valid
            // Since this is just for display, we can provide a string that is
            // not an actual Uri with only the fields that are specified.
            // request.GetDisplayUrl(), used above, will throw an exception
            // if request.Host is null.
            return $"{request.Scheme}://{NoHostSpecified}{request.PathBase.Value}{request.Path.Value}{request.QueryString.Value}";
        }

        private bool ShouldIgnore(HttpContext httpContext)
        {
            foreach (Func<HttpContext, bool> ignore in _options.IgnorePatterns)
            {
                if (ignore(httpContext))
                    return true;
            }

            return false;
        }
    }
}
