namespace LogsShorterer.Writer
{
    public class ConsolehWriter : IWriter
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }
    }
}
