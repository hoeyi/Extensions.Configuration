using Microsoft.Extensions.Logging;

namespace MSTest
{
    /// <summary>
    /// Contains variables scoped to the Configuration.Extensions.MSTest assembly. 
    /// </summary>
    class MSTest
    {
        public static readonly ILogger Logger = LoggerFactory
            .Create(builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug))
                .CreateLogger<MSTest>();
    }
}
