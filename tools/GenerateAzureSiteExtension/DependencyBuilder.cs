using System;
using System.IO;
using Datadog.Core.Tools;

namespace PrepareRelease
{
    public static class DependencyBuilder
    {
        private static readonly string Release = "Release";
        private static readonly string X64 = "x64";
        private static readonly string X86 = "x86";

        private static readonly string SourceDirectory = Path.Combine(EnvironmentTools.GetSolutionDirectory(), "src");
        private static readonly string NativeProj = Path.Combine(SourceDirectory,  "Datadog.Trace.ClrProfiler.Native", "Datadog.Trace.ClrProfiler.Native.DLL.vcxproj");
        private static readonly string TraceProj = Path.Combine(SourceDirectory,  "Datadog.Trace", "Datadog.Trace.csproj");
        private static readonly string AutomaticInstrumentationProj = Path.Combine(SourceDirectory,  "Datadog.Trace.ClrProfiler.Managed", "Datadog.Trace.ClrProfiler.Managed.csproj");
        private static readonly string AspNetModuleProj = Path.Combine(SourceDirectory,  "Datadog.Trace.AspNet", "Datadog.Trace.AspNet.csproj");

        public static void Run()
        {
            BuildAndWait(NativeProj, Release, X86);

            BuildAndWait(NativeProj, Release, X64);

            BuildAndWait(TraceProj, Release, X64);

            BuildAndWait(AutomaticInstrumentationProj, Release, X64);

            BuildAndWait(AspNetModuleProj, Release, X64);
        }

        private static void BuildAndWait(string projectFile, string buildConfiguration, string architecture)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            // startInfo.EnvironmentVariables.Add("VCTargetsPath", "C:\Program Files (x86)\MSBuild\Microsoft.Cpp\v4.0\V120");
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C dotnet msbuild {projectFile} -p:Configuration={buildConfiguration} -p:Platform={architecture}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(10_000);

            if (process.ExitCode != 0)
            {
                throw new Exception("Non-zero exit code on command");
            }
        }
    }
}
