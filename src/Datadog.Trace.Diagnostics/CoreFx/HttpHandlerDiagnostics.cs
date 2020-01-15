using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Datadog.Trace.Diagnostics.Internal;

namespace Datadog.Trace.Diagnostics.CoreFx
{
    /// <summary>
    /// Instruments outgoing HTTP calls that use <see cref="HttpClientHandler"/>.
    /// <para/>See https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs
    /// <para/>and https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/DiagnosticsHandlerLoggingStrings.cs
    /// </summary>
    internal sealed class HttpHandlerDiagnostics : DiagnosticListenerObserver
    {
        public const string DiagnosticListenerName = "HttpHandlerDiagnosticListener";

        private const string PropertiesKey = "Datadog-Span";

        private static readonly PropertyFetcher _activityStart_RequestFetcher = new PropertyFetcher("Request");
        private static readonly PropertyFetcher _activityStop_RequestFetcher = new PropertyFetcher("Request");
        private static readonly PropertyFetcher _activityStop_ResponseFetcher = new PropertyFetcher("Response");
        private static readonly PropertyFetcher _activityStop_RequestTaskStatusFetcher = new PropertyFetcher("RequestTaskStatus");
        private static readonly PropertyFetcher _exception_RequestFetcher = new PropertyFetcher("Request");
        private static readonly PropertyFetcher _exception_ExceptionFetcher = new PropertyFetcher("Exception");

        private readonly HttpHandlerDiagnosticOptions _options;

        protected override string GetListenerName() => DiagnosticListenerName;

        public HttpHandlerDiagnostics(ILoggerFactory loggerFactory, IDatadogTracer tracer,
            IOptions<HttpHandlerDiagnosticOptions> options, IOptions<GenericEventOptions> genericEventOptions)
            : base(loggerFactory, tracer, genericEventOptions.Value)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _options.IgnorePatterns.Add(
                requestMessage =>
                {
                    if (requestMessage.Headers.TryGetValues(HttpHeaderNames.TracingEnabled, out var headerValues))
                    {
                        if (headerValues.Any(s => string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)))
                        {
                            // tracing is disabled for this request via http header,
                            // return true to ignore request
                            return true;
                        }
                    }

                    return false;
                });
        }

        protected override void OnNext(string eventName, object arg)
        {
            switch (eventName)
            {
                case "System.Net.Http.HttpRequestOut.Start":
                    {
                        var request = (HttpRequestMessage)_activityStart_RequestFetcher.Fetch(arg);
                        Uri requestUri = request.RequestUri;

                        if (IgnoreRequest(request))
                        {
                            Logger.LogDebug("Ignoring Request {RequestUri}", requestUri);
                            return;
                        }

                        string operationName = _options.OperationNameResolver(request);
                        Span span = Tracer.StartSpan(operationName);

                        span.SetTag(Tags.SpanKind, SpanKinds.Client)
                            .SetTag(Tags.InstrumentationName, _options.ComponentName)
                            .SetTag(Tags.HttpMethod, request.Method.ToString())
                            .SetTag(Tags.HttpUrl, requestUri.ToString())
                            .SetTag(Tags.OutHost, requestUri.Host)
                            .SetTag(Tags.OutPort, requestUri.Port.ToString());

                        Scope scope = Tracer.ActivateSpan(span);

                        _options.OnRequest?.Invoke(span, request);

                        if (_options.InjectEnabled?.Invoke(request) ?? true)
                        {
                            // TODO lucas
                            // Tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new HttpHeadersInjectAdapter(request.Headers));
                        }

                        // This throws if there's already an item with the same key. We do this for now to get notified of potential bugs.
                        request.Properties.Add(PropertiesKey, scope);
                    }
                    break;

                case "System.Net.Http.Exception":
                    {
                        var request = (HttpRequestMessage)_exception_RequestFetcher.Fetch(arg);

                        if (request.Properties.TryGetValue(PropertiesKey, out object objScope) && objScope is Scope scope)
                        {
                            var exception = (Exception)_exception_ExceptionFetcher.Fetch(arg);

                            scope.Span.SetException(exception);

                            _options.OnError?.Invoke(scope.Span, exception, request);
                        }
                    }
                    break;

                case "System.Net.Http.HttpRequestOut.Stop":
                    {
                        var request = (HttpRequestMessage)_activityStop_RequestFetcher.Fetch(arg);

                        if (request.Properties.TryGetValue(PropertiesKey, out object objScope) && objScope is Scope scope)
                        {
                            var response = (HttpResponseMessage)_activityStop_ResponseFetcher.Fetch(arg);
                            var requestTaskStatus = (TaskStatus)_activityStop_RequestTaskStatusFetcher.Fetch(arg);

                            if (response != null)
                            {
                                scope.Span.SetTag(Tags.HttpStatusCode, ((int)response.StatusCode).ToString());
                            }

                            if (requestTaskStatus == TaskStatus.Canceled || requestTaskStatus == TaskStatus.Faulted)
                            {
                                scope.Span.Error = true;
                            }

                            scope.Close();
                            request.Properties.Remove(PropertiesKey);
                        }
                    }
                    break;
            }
        }

        private bool IgnoreRequest(HttpRequestMessage request)
        {
            foreach (Func<HttpRequestMessage, bool> ignore in _options.IgnorePatterns)
            {
                if (ignore(request))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
