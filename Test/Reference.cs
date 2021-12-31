using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Extensions.Configuration.Test
{
    /// <summary>
    /// Contains variables scoped to the Extensions.Configuration.Test assembly. 
    /// </summary>
    class Reference
    {
        /// <summary>
        /// Gets the unit-test project <see cref="ILogger"/> instance.
        /// </summary>
        public static readonly ILogger Logger = LoggerFactory
            .Create(builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug)
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug))
            .CreateLogger<Reference>();

        /// <summary>
        /// Gets the name of the unit-test assembly.
        /// </summary>
        public static string AssemblyName
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }
    }
}
