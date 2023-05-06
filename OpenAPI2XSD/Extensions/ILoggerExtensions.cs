using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;

namespace OpenAPI2XSD.Extensions
{
    public static class ILoggerExtensions
    {
        public static void LogObject(this ILogger logger, object obj, LogLevel logLevel = LogLevel.Debug, bool indented = true)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = indented };
            
            string json = JsonSerializer.Serialize(obj, options);
            string[] lines = json.Split('\n').Select(x => x.Replace("\r", "")).ToArray();

            foreach (string line in lines)
            {
                logger.Log(logLevel, line);
            }
        }
    }
}
