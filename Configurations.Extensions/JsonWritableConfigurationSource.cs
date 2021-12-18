using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// Represents a JSON file as an <see cref="IConfigurationSource"/>, for 
    /// use with <see cref="JsonWritableConfigurationProvider"/>.
    /// </summary>
    public class JsonWritableConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider ??= builder.GetFileProvider();
            return new JsonWritableConfigurationProvider(this);
        }
    }
}
