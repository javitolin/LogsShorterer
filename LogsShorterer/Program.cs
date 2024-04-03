using LogsShorterer.Entities;
using LogsShorterer.Writer;

namespace LogsShorterer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LogEntity logEntity = new LogEntity
            {
                Message = "HELLO",
                AnotherString = "AAAAAAAAAAAAAAAAAA",
                Description = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                ExceptionMessage = string.Join("", Enumerable.Repeat("ABC", 350).ToArray()),
                keyValuePairs = new Dictionary<string, string>
                {
                    ["FirstKey"] = "FirstValue"
                }
            };

            IWriter writer = new ConsoleLengthWriter();
            ILogWriter logWriter = new LogWriter(writer, new string[] { "Level", "Message" });
            logWriter.PrintLogEntity(logEntity, 80);
        }
    }
}