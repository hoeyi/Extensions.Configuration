using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

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
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonWritable(
            this IConfigurationBuilder builder, string path, bool optional = true, bool reloadOnChange = true)
        {
            return builder.AddJsonWritable(s =>
            {
                s.FileProvider = null;
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ResolveFileProvider();
            });
        }

        /// <summary>
        /// Adds a writable JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonWritable(
            this IConfigurationBuilder builder, Action<JsonWritableConfigurationSource> configureSource)
        {
            return builder.Add(configureSource);
        }
    }
}
