using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Datadog.Trace.Diagnostics.Internal;

namespace Datadog.Trace.Diagnostics.CoreFx
{
    internal sealed class SqlClientDiagnostics : DiagnosticListenerObserver
    {
        public const string DiagnosticListenerName = "SqlClientDiagnosticListener";

        private static readonly PropertyFetcher _activityCommand_RequestFetcher = new PropertyFetcher("Command");
        private static readonly PropertyFetcher _exception_ExceptionFetcher = new PropertyFetcher("Exception");

        private readonly SqlClientDiagnosticOptions _options;

        public SqlClientDiagnostics(ILoggerFactory loggerFactory, IDatadogTracer tracer, IOptions<SqlClientDiagnosticOptions> options,
            IOptions<GenericEventOptions> genericEventOptions)
           : base(loggerFactory, tracer, genericEventOptions.Value)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override string GetListenerName() => DiagnosticListenerName;

        protected override void OnNext(string eventName, object untypedArg)
        {
            switch (eventName)
            {
                case "System.Data.SqlClient.WriteCommandBefore":
                    {
                        var args = (SqlCommand)_activityCommand_RequestFetcher.Fetch(untypedArg);
                        string operationName = _options.OperationNameResolver(args);

                        Span span = Tracer.StartSpan(operationName)
                                          .SetTag(Tags.SpanKind, SpanKinds.Client)
                                          .SetTag(Tags.InstrumentationName, _options.ComponentName)
                                          .SetTag(Tags.DbName, args.Connection.Database)
                                          .SetTag(Tags.SqlQuery, args.CommandText);

                        Scope scope = Tracer.ActivateSpan(span);
                    }
                    break;

                case "System.Data.SqlClient.WriteCommandError":
                    {
                        Exception ex = (Exception)_exception_ExceptionFetcher.Fetch(untypedArg);

                        DisposeActiveScope(isScopeRequired: true, exception: ex);
                    }
                    break;

                case "System.Data.SqlClient.WriteCommandAfter":
                    {
                        DisposeActiveScope(isScopeRequired: true);
                    }
                    break;
            }
        }
    }
}
