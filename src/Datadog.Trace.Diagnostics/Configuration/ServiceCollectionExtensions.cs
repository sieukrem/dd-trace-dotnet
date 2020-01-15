using System;
using Datadog.Trace;
using Datadog.Trace.Diagnostics;
using Datadog.Trace.Diagnostics.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Datadog.Trace.Diagnostics.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenTracing instrumentation for ASP.NET Core, CoreFx (BCL), Entity Framework Core.
        /// </summary>
        public static IServiceCollection AddDatadogTracing(this IServiceCollection services, Action<IOpenTracingBuilder> builder = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddDatadogCoreServices(
                ddBuilder =>
                {
                    ddBuilder.AddAspNetCore()
                             .AddCoreFx()
                             .AddEntityFrameworkCore();
                    //.AddLoggerProvider();

                    builder?.Invoke(ddBuilder);
                });
        }

        /// <summary>
        /// Adds the core services required for OpenTracing without any actual instrumentations.
        /// </summary>
        public static IServiceCollection AddDatadogCoreServices(this IServiceCollection services, Action<IOpenTracingBuilder> builder = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IDatadogTracer>(Tracer.Instance);
            services.TryAddSingleton<Tracer>(Tracer.Instance);
            services.TryAddSingleton<IGlobalTracerAccessor, GlobalTracerAccessor>();

            services.TryAddSingleton<DiagnosticManager>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, InstrumentationService>());

            var builderInstance = new OpenTracingBuilder(services);

            builder?.Invoke(builderInstance);

            return services;
        }
    }
}
