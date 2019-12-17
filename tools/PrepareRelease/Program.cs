using System;
using System.Linq;

namespace PrepareRelease
{
    public class Program
    {
        public const string Versions = "versions";
        public const string Integrations = "integrations";
        public const string Msi = "msi";

        public static void Main(string[] args)
        {
            if (JobShouldRun(Integrations, args))
            {
                GenerateIntegrationDefinitions.Run();
            }

            if (JobShouldRun(Versions, args))
            {
                SetAllVersions.Run();
            }

            if (JobShouldRun(Msi, args))
            {
                // SyncMsiContent.Run();
            }
        }

        private static bool JobShouldRun(string jobName, string[] args)
        {
            return args.Length == 0 || args.Any(a => string.Equals(a, jobName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
