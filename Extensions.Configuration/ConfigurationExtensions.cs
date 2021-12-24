﻿using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// Proviates extension methods for classes in <see cref="Microsoft.Extensions.Configuration"/> namespace.
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
        /// Rotates encryption for values in RSA protected configuration providers.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfigurationRoot"/>.</param>
        /// <param name="keyContainerName">The RSA key container name.</param>
        /// <returns>True if the operation is successful, else false.</returns>
        public static bool RotateKey(this IConfigurationRoot configuration, string keyContainerName)
        {
            var rsaProviders = configuration.Providers.Where(p =>
                typeof(IRSAProtectedConfigurationProvider)
                    .IsAssignableFrom(
                        p.GetType()))
                    .Cast<IRSAProtectedConfigurationProvider>().ToArray();

            var results = rsaProviders.Select(rsa => rsa.RotateKey(newKeyContainer: keyContainerName));

            var allSuccessful = results.All(r => r);

            return allSuccessful;
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
            var jsonSource = new JsonWritableConfigurationSource()
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
        /// <param name="encryptionKeyContainer">The name of an RSA key container to use for asymmetric encryption.</param>
        /// <param name="logger">An <see cref="ILogger"/> that represents an application logging implementation.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddSecureJsonWritable(
            this IConfigurationBuilder builder,
            string path,
            string encryptionKeyContainer,
            ILogger logger,
            bool optional = false,
            bool reloadOnChange = true)
        {
            var jsonSource = new JsonWritableConfigurationSource(
                    keyContainerName: encryptionKeyContainer,
                    logger: logger)
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