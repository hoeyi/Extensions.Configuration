using Microsoft.Extensions.Configuration;
using System;

namespace Ichsoft.Configuration.Extensions
{
    public class DecryptedConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            throw new NotImplementedException();
            //return new EncryptedConfiguration();
        }
    }
}
