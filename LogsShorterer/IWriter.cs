using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogsShorterer
{
    public interface IWriter
    {
        void Write(string message);
    }

    public class ConsoleWriter : IWriter
    {
        public void Write(string message)
        {
            Console.WriteLine($"Length: [{message.Length}], {message}");
        }
    }
}