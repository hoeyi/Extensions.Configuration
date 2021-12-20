using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// Extension methods for classes in <see cref="Microsoft.Extensions.Configuration"/> namespace.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Commits the current key-value pairs for each provider that implements <see cref="IWritableConfigurationProvider"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public static void Commit(this IConfigurationRoot configuration)
        {
            var writableProviders = configuration.Providers.Where(p =>
                typeof(IWritableConfigurationProvider).IsAssignableFrom(p.GetType())).Cast<IWritableConfigurationProvider>().ToArray();

            foreach (var provider in writableProviders)
            {
                provider.Commit();
            }
        }

        /// <summary>
        /// Adds writable JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <param name="encryptionKeyContainer">The name of the key container to use for saving and accessing public/private keys.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonWritable(
            this IConfigurationBuilder builder, 
            string path, 
            bool optional = true, 
            bool reloadOnChange = true,
            string encryptionKeyContainer = null,
            ILogger logger = null)
        {
            var jsonSource = string.IsNullOrEmpty(encryptionKeyContainer) ?
                new JsonWritableConfigurationSource()
                {
                    FileProvider = null,
                    Path = path,
                    Optional = optional,
                    ReloadOnChange = true
                } :
                new JsonWritableConfigurationSource(
                    keyContainerName: encryptionKeyContainer,
                    logger: logger)
                {
                    FileProvider = null,
                    Path = path,
                    Optional = optional,
                    ReloadOnChange = true
                };

            jsonSource.ResolveFileProvider();

            return builder.Add(source: jsonSource);
        }
    }
}
