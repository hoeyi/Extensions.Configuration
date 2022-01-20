using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ichosoft.Extensions.Configuration.UnitTest.Setup
{
    /// <summary>
    /// Contains variables scoped to the Ichosoft.Extensions.Configuration.UnitTest assembly. 
    /// </summary>
    class Global
    {
        /// <summary>
        /// Gets the unit-test project <see cref="ILogger"/> instance.
        /// </summary>
        public static readonly ILogger Logger = LoggerFactory
            .Create(builder => builder
                .AddConsole()
                .AddDebug())
            .CreateLogger<Global>();

        /// <summary>
        /// Gets the name of the unit-test assembly.
        /// </summary>
        public static string AssemblyName
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }

        /// <summary>
        /// Generates random Lorem-Ipsum string according to the given parameters.
        /// </summary>
        /// <param name="minimumWords"></param>
        /// <param name="maximumWords"></param>
        /// <param name="minimumSentences"></param>
        /// <param name="maximumSentences"></param>
        /// <param name="paragraphCount"></param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string LoremIpsum(int minimumWords, int maximumWords,
            int minimumSentences, int maximumSentences,
            int paragraphCount)
        {

            var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
                "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
                "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

            var rand = new Random();
            int numSentences = rand.Next(maximumSentences - minimumSentences)
                + minimumSentences + 1;
            int numWords = rand.Next(maximumWords - minimumWords) + minimumWords + 1;

            StringBuilder result = new();

            for (int p = 0; p < paragraphCount; p++)
            {
                result.Append("<p>");
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0)
                        { result.Append(' '); }
                        result.Append(words[rand.Next(words.Length)]);
                    }
                    result.Append(". ");
                }
                result.Append("</p>");
            }

            return result.ToString();
        }
    }
}
