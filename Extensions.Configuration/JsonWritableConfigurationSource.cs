using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// Represents a JSON file as an <see cref="IConfigurationSource"/>, for 
    /// use with <see cref="JsonWritableConfigurationProvider"/>.
    /// </summary>
    class JsonWritableConfigurationSource : JsonConfigurationSource
    {
        private readonly bool protectedSource;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of <see cref="JsonWritableConfigurationSource"/>.
        /// </summary>
        private JsonWritableConfigurationSource()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="JsonWritableConfigurationSource"/>.
        /// </summary>
        /// <param name="useProtectedSource"></param>
        public JsonWritableConfigurationSource(bool useProtectedSource)
            : this()
        {
            protectedSource = useProtectedSource;
        }

        public JsonWritableConfigurationSource(bool useProtectedSource, ILogger logger)
            : this(useProtectedSource)
        {
            this.logger = logger;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider ??= builder.GetFileProvider();

            // If a protected source use secure/writable provider.
            if (protectedSource)
                return new JsonSecureWritableConfigurationProvider(this, logger);
            else
                return new JsonWritableConfigurationProvider(this);
        }
    }
}
