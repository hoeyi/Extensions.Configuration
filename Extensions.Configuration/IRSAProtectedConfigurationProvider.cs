namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// Represents a configuration provider that encrypts in-memory and persisted values.
    /// </summary>
    interface IRSAProtectedConfigurationProvider
    {
        /// <summary>
        /// Deletes the current key attached to this <see cref="IRSAProtectedConfigurationProvider"/>.
        /// </summary>
        /// <returns>True if the operation is successful, else false.</returns>
        bool DeleteKey();

        /// <summary>
        /// Rotates the key used to encrypt the values in the <see cref="IRSAProtectedConfigurationProvider"/> object.
        /// </summary>
        /// <param name="newKeyContainer">The name of a new RSA key container.</param>
        /// <param name="deleteOnSuccess">True to delete the old key container, else false.</param>
        /// <returns>True if the operation is successful, else false.</returns>        
        bool RotateKey(string newKeyContainer, bool deleteOnSuccess = true);
    }
}
