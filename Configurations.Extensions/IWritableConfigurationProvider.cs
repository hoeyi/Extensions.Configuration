namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// Provides persistable key/values for an application.
    /// </summary>
    interface IWritableConfigurationProvider
    {
        /// <summary>
        /// Commits all in-memory changes to a persistent store.
        /// </summary>
        void Commit();
    }
}
