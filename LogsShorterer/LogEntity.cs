using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LogsShorterer
{
    public class LogEntity
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ExceptionMessage { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AnotherString { get; set; } = string.Empty;
        public string YetAnotherString { get; set; } = string.Empty;
        public Dictionary<string, string> keyValuePairs { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
