using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using System;

namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// Represents a JSON file as an <see cref="IConfigurationSource"/>, for 
    /// use with <see cref="JsonWritableConfigurationProvider"/>.
    /// </summary>
    class JsonWritableConfigurationSource : JsonConfigurationSource
    {
        private readonly string keyContainerName;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of <see cref="JsonWritableConfigurationSource"/>.
        /// </summary>
        public JsonWritableConfigurationSource()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="JsonWritableConfigurationSource"/> using 
        /// <paramref name="keyContainerName"/> as the key container for encrypting and 
        /// decrypting values.
        /// </summary>
        /// <param name="keyContainerName">The RSA key container name.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public JsonWritableConfigurationSource(string keyContainerName, ILogger logger)
            : base()
        {
            if (string.IsNullOrEmpty(keyContainerName))
                throw new ArgumentNullException(paramName: nameof(keyContainerName));

            if (logger is null)
                throw new ArgumentNullException(paramName: nameof(logger));

            this.keyContainerName = keyContainerName;
            this.logger = logger;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider ??= builder.GetFileProvider();

            // If key container is not provided then use unencrypted variant.
            if (string.IsNullOrEmpty(keyContainerName))
                return new JsonWritableConfigurationProvider(this);
            else
                return new JsonWritableConfigurationProvider(
                    source: this,
                    keyContainername: keyContainerName,
                    logger: logger);
        }
    }
}
