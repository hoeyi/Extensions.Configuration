namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// Provides static helper methods for working with strings.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Converts the string to a log template string by replacing braced integers.
        /// </summary>
        /// <param name="template">The string template.</param>
        /// <param name="parameterNames">The string names of the parameters to use in loggging.</param>
        /// <returns>A string template suitable for structured logging.</returns>
        /// <example>
        ///     var template = "{0} {1}";
        ///     var logTemplate = template.ConvertToLogTemplate("Property1", "Property2");
        ///     
        ///     // logTemplate is now: "{Property1} {Property2}"
        /// </example>
        public static string ConvertToLogTemplate(this string template, params string[] parameterNames)
        {
            if (string.IsNullOrEmpty(template))
                return null;

            // If no parameters are provided for interpolation, return the original template.
            if (parameterNames is null || parameterNames.Length == 0)
                return template;

            // Declare a return variable, and loop through parameters to replace values in original template.
            string logTemplate = template;
            for(int i = 0; i < parameterNames.Length; i++)
            {
                logTemplate = logTemplate.Replace($"{{{i}}}", $"{{{parameterNames[i]}}}");
            }

            return logTemplate;
        }
    }
}
