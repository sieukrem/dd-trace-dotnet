using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Datadog.Trace.ClrProfiler.Managed.Loader
{
    /// <summary>
    /// A class that attempts to load the Datadog.Trace.ClrProfiler.Managed .NET assembly.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Initializes static members of the <see cref="Startup"/> class.
        /// This method also attempts to load the Datadog.Trace.ClrProfiler.Managed. NET assembly.
        /// </summary>
        static Startup()
        {
            StartTraceAgent();
            ManagedProfilerDirectory = ResolveManagedProfilerDirectory();
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve_ManagedProfilerDependencies;
            TryLoadManagedAssembly();
        }

        internal static string ManagedProfilerDirectory { get; }

        private static bool TryLoadManagedAssembly()
        {
            try
            {
                Assembly.Load(new AssemblyName("Datadog.Trace.ClrProfiler.Managed, Version=1.10.33.0, Culture=neutral, PublicKeyToken=def86d061d0d2eeb"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StartTraceAgent()
        {
            Action<string> appendToLog = null;

            try
            {
                var azureAppServicesValue = Environment.GetEnvironmentVariable("DD_AZURE_APP_SERVICES");

                if (azureAppServicesValue == "1")
                {
                    var path = Environment.GetEnvironmentVariable("DD_TRACE_AGENT_PATH");

                    if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = path, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardError = true,
                        };

                        var process = Process.Start(startInfo);

                        var logPathVariable = Environment.GetEnvironmentVariable("DD_TRACE_LOG_PATH");

                        var logFolder = Path.GetDirectoryName(logPathVariable);

                        var nestedTraceAgentLog = Path.Combine(logFolder, "nested-trace-agent.log");

                        if (!File.Exists(nestedTraceAgentLog))
                        {
                            File.Create(nestedTraceAgentLog);
                        }

                        appendToLog = (text) => File.AppendAllText(nestedTraceAgentLog, text);

                        process.OutputDataReceived += (sender, args) =>
                        {
                            appendToLog(args.Data);
                        };

                        process.BeginOutputReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                appendToLog?.Invoke(ex.ToString());
            }
        }
    }
}
