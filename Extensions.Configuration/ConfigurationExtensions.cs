using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Ichosys.Extensions.Configuration.Test")]

namespace Ichosys.Extensions.Configuration
{
    /// <summary>
    /// Proviates extension methods for classes in <see cref="Microsoft.Extensions.Configuration"/> namespace.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Commits the current key-value pairs for each provider that implements <see cref="IWritableConfigurationProvider"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfigurationRoot"/>.</param>
        public static void Commit(this IConfigurationRoot configuration)
        {
            var writableProviders = configuration.Providers.Where(p =>
                    typeof(IWritableConfigurationProvider)
                    .IsAssignableFrom(p.GetType()))
                .Cast<IWritableConfigurationProvider>().ToArray();

            foreach (var provider in writableProviders)
            {
                provider.Commit();
            }
        }

        /// <summary>
        /// Rotates encryption for values in RSA protected configuration providers.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfigurationRoot"/>.</param>
        /// <param name="keyContainerName">The RSA key container name.</param>
        /// <returns>True if the operation is successful, else false.</returns>
        public static bool RotateKey(this IConfigurationRoot configuration, string keyContainerName)
        {
            var rsaProviders = configuration.Providers.Where(p =>
                    typeof(IRSAProtectedConfigurationProvider)
                    .IsAssignableFrom(p.GetType()))
                .Cast<IRSAProtectedConfigurationProvider>()
                .ToList();

            if (rsaProviders.Count == 0)
                return false;

            bool result = rsaProviders.TrueForAll(rsa => rsa.RotateKey(newKeyContainer: keyContainerName));

            return result;
        }

        /// <summary>
        /// Deletes the given key container.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfigurationRoot"/>.</param>
        /// <param name="keyContainerName">The RSA key container name.</param>
        /// <returns>True if the operation is successful, else false.</returns>
        public static bool DeleteKey(this IConfigurationRoot configuration, string keyContainerName)
        {
            var rsaProviders = configuration.Providers.Where(p =>
                    typeof(IRSAProtectedConfigurationProvider)
                    .IsAssignableFrom(p.GetType()))
                .Cast<IRSAProtectedConfigurationProvider>()
                .Where(p => p.KeyContainerName == keyContainerName)
                .ToList();

            if (rsaProviders.Count == 0)
                return false;

            bool result = rsaProviders.TrueForAll(p => p.DeleteKey());

            return result;
        }

        /// <summary>
        /// Adds writable JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonWritable(
            this IConfigurationBuilder builder,
            string path,
            bool optional = true,
            bool reloadOnChange = true)
        {
            var jsonSource = new JsonWritableConfigurationSource(useProtectedSource: false)
            {
                FileProvider = null,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };

            jsonSource.ResolveFileProvider();

            return builder.Add(source: jsonSource);
        }

        /// <summary>
        /// Adds writable JSON configuration source with encrypted values to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSecureJsonWritable(
            this IConfigurationBuilder builder,
            string path,
            ILogger logger = null,
            bool optional = false,
            bool reloadOnChange = true)
        {
            var jsonSource = logger is null ?
                new JsonWritableConfigurationSource(useProtectedSource: true)
                {
                    FileProvider = null,
                    Path = path,
                    Optional = optional,
                    ReloadOnChange = reloadOnChange
                } :
                new JsonWritableConfigurationSource(useProtectedSource: true, logger)
                {
                    FileProvider = null,
                    Path = path,
                    Optional = optional,
                    ReloadOnChange = reloadOnChange
                };

            jsonSource.ResolveFileProvider();

            return builder.Add(source: jsonSource);
        }
    }
}
