# Ichsoft.Configuration.Extensions
`Ichsoft.Configuration.Extensions` extends the [.NET Configuration API](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) by adding custom providers for writing configurations to disk and encrypting values in-memory and at rest.

## Example snippets ##
Use `IConfigurationBuilder` extension methods `AddJsonWritable` and `AddSecureJsonWritable` to configure custom providers.

#### Configure a JSON-writable source without encryption.
```CSharp
// Create example configuration and access the property 'InstallDate'.

// Build configuration.
IConfigurationRoot unprotectedConfig = new ConfigurationBuider()
        .AddSecureJsonWritable(
            path: "appsettings.json",
            optional: true,
            reloadOnChange: false);

var installDate = unprotectedConfig["InstallDate"];

Console.WriteLine($"Install date: {installDate}");
```
####

#### Configure a JSON-writable source with encryption. 
Values in protected configuration providers are kept as cipher text within the configuration data and when saved to a data store, if it applies. Values are only decrypted when accessed.

```CSharp
// Create example configuration using an RSA key container name and access 
// property 'ConnectionStrings.Production'.

// Create ILogger instance.
ILogger logger = LoggerFactory.Create(b => b).CreateLogger<Program>();

// Build configuration.
IConfigurationRoot protectedConfig = new ConfigurationBuider()
        .AddSecureJsonWritable(
            path: "appsettings.protected.json",
            optional: false,
            reloadOnChange: false,
            encryptionKeyContainer: "MyKeyContainer",
            logger: logger); 

var connString = protectedConfig["ConnectionsStrings:Production"];

Console.WriteLine($"Connection string (prod): {connString}");
```
####

#### Rotating the encryption key.
Providers implementing `IRSAProtectedConfigurationProvider` allow for rotating all encrypted values to a new key.

```CSharp
// Configurate with encrypted values and rotate the keys to a new key container.
IConfigurationRoot protectedConfig = new ConfigurationBuider()
        .AddSecureJsonWritable(
            path: "appsettings.protected.json",
            optional: false,
            reloadOnChange: true,
            encryptionKeyContainer: "MyKeyContainer",
            logger: logger); 

bool result = protectedConfig.RotateKey(keyContainerName: "MyNewKeyContainer");

Console.WriteLine($"Key rotated: {result}");
```
####

#### Saving values to a file.
Providers implementing `IWritableConfigurationProvider` allow for saving all values according in the manner prescribed by the provider.

```CSharp
// Configuration with encrypted values, set a value, then save the 
// configuration to 'appsettings.protected.json'.

// Build configuration.
IConfigurationRoot protectedConfig = new ConfigurationBuider()
        .AddSecureJsonWritable(
            path: "appsettings.protected.json",
            optional: false,
            reloadOnChange: true,
            encryptionKeyContainer: "MyKeyContainer",
            logger: logger);

// Set 'ConnectionStrings:Production'
protectedConfig["ConnectionStrings:Production"] = "DataSource=...";

// Save the update value to a JSON file.
protectedConfig.Commit();
```
####

#