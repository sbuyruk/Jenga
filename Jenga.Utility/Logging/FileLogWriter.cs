using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{

    public class FileLogWriter : ILogWriter
    {
        private readonly string _path;

        public FileLogWriter(string path = "Logs/jenga-log.txt")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            _path = path;
        }

        public void Write(LogLevel level, string message)
        {
            var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            File.AppendAllText(_path, log + Environment.NewLine);
        }
    }


}
