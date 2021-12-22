namespace Hoeyi.Configuration.Extensions
{
    /// <summary>
    /// Provides persistable key/values for an application.
    /// </summary>
    interface IWritableConfigurationProvider
    {
        /// <summary>
        /// Saves all in-memory configuration changes to the 
        /// <see cref="IWritableConfigurationProvider"/> data store.
        /// </summary>
        void Commit();
    }
}
