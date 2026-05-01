using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    /// <summary>
    /// Thread-safe file writer with daily rotation.
    /// Default klasör: Logs/, dosya adı: jenga-yyyy-MM-dd.log
    /// </summary>
    public class FileLogWriter : ILogWriter
    {
        private readonly string _directory;
        private readonly string _fileNamePrefix;
        private static readonly Lock _gate = new();

        public FileLogWriter(string directory = "Logs", string fileNamePrefix = "jenga")
        {
            _directory = directory;
            _fileNamePrefix = fileNamePrefix;
            Directory.CreateDirectory(_directory);
        }

        public void Write(LogLevel level, string message)
        {
            var now = DateTime.Now;
            var path = Path.Combine(_directory, $"{_fileNamePrefix}-{now:yyyy-MM-dd}.log");
            var line = $"{now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

            lock (_gate)
            {
                File.AppendAllText(path, line);
            }
        }
    }
}
