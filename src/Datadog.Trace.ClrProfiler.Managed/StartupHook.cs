using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace.ClrProfiler
{
    /// <summary>
    /// Start-up hook used to initialize instrumentation in .NET Core 3+
    /// </summary>
    public static class StartupHook
    {
        /// <summary>
        /// Called by the runtime to initialize instrumentation in .NET Core 3+
        /// </summary>
        public static void Initialize()
        {
            throw new Exception("INITIALIZE");
        }
    }
}
