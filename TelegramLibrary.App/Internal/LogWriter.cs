using System;
using System.IO;

namespace TelegramLibrary.App.Internal
{
    public class LogWriter
    {
        private readonly string _basePath;

        public LogWriter(string basePath)
        {
            _basePath = basePath;
        }

        public void Write(string log)
        {
            using (var streamWriter = new StreamWriter($"{_basePath}/log.txt", append: true))
            {
                streamWriter.WriteLine($"{DateTime.Now:yyyy-mm-dd HH:mm:ss} -> {log}");
            }
        }

        public void Write(string log, Exception ex)
        {
            Write($"{log} -> {ex.GetType().Name}:{ex.Message}");
        }
    }
}
