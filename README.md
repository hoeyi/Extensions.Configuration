# Hoeyi.Extensions.Configuration #
`Hoeyi.Extensions.Configuration` extends the [.NET Configuration API](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) by adding custom providers for writing configurations to disk and encrypting values in-memory and at rest.

* [Building Project](#building-project)
* [Code Examples](#code-examples)
* [Commit Message Guidelines](#commit-message-guidelines)

## Building Project ##
By default, the project `$(BuildNumber)` property is set to zero. Uncomment the line `Extensions.Configuration.csproj` to allow auto-assignment of the build number. Once the build is complete, revert the change to the `$(BuilderNumber)` property assignment.

<br/>

## Code Examples ##
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

<br/>
## Commit Message Guidelines ##

The structure of these guidelines are based on the [Angular convetion](
https://github.com/angular/angular/blob/22b96b9/CONTRIBUTING.md#commit) and 
[Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0-beta.2/).

Commit messages should follow the format:
```
<type>[optional scope]: <description>
[optional body]
[optional footer]
```

### Type ###
Must be one of the following:

* **build**: Changes that affect the build system or external dependencies
* **docs**: Documentation only changes
* **feat**: A new feature
* **fix**: A bug fix
* **perf**: A code change that improves performance
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **revert**: Reverts commit `<hash>`.
* **style**: Changes that do not affect the meaning of the code 
(white-space, formatting, missing semi-colons, etc)
* **test**: Adding missing tests or correcting existing tests

### Scope ###
Scope refers to the domain of the code changed. Choose from the following:
* **Cryptography**: Cryptographic and key management methods.
* **Extensions**: Extension and helper methods for the .NET Configuration API.

Example: 
```
feat(Cryptography): add support for Azure key vault

fix(Extensions): fix builder for writable XML source
```

### Subject ###
The subject contains a succinct description of the change:

* Use the imperative, present tense
* Don't capitalize the first letter
* Don't include punctuation

### Body ###
The body contains the detail of why the change was made:
* Use the imperative, present tense
* Include the motivation for the change with contrast to prior behavior

### Footer ###
The footer contains information on breaking changes. Start with the phrase 
`BREAKING CHANGE:`. Also use this space to reference closing GitHub issues. 

Example:
```
BREAKING CHANGE: Ends support for [NAME] API

Resolves #42 (where #42 is the GitHub issue no.)
```

