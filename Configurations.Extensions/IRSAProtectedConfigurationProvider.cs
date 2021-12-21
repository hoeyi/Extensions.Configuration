using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// Represents a configuration provider that encrypts in-memory and persisted values.
    /// </summary>
    interface IRSAProtectedConfigurationProvider
    {

        /// <summary>
        /// Rotates the key used to encrypt the values in the <see cref="IRSAProtectedConfigurationProvider"/> object.
        /// </summary>
        /// <param name="newKeyContainer">The name of a new RSA key container.</param>
        /// <param name="deleteOnSuccess">True to delete the old key container, else false.</param>
        /// <returns>True if the operation is successful, else false.</returns>        
        bool RotateKey(string newKeyContainer, bool deleteOnSuccess = true);
    }
}
