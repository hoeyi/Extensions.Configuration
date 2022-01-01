using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Hoeyi.Extensions.Configuration.UnitTest.Setup
{
    /// <summary>
    /// Contains variables scoped to the Hoeyi.Extensions.Configuration.UnitTest assembly. 
    /// </summary>
    class Global
    {
        /// <summary>
        /// Gets the unit-test project <see cref="ILogger"/> instance.
        /// </summary>
        public static readonly ILogger Logger = LoggerFactory
            .Create(builder => builder
                .AddConsole()
                .AddDebug())
            .CreateLogger<Global>();

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
