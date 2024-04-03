namespace LogsShorterer.Writer
{
    public class ConsoleLengthWriter : IWriter
    {
        public void Write(string message)
        {
            Console.WriteLine($"Length: [{message.Length}], {message}");
        }
    }
}
