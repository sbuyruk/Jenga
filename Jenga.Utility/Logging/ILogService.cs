using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    public interface ILogService
    {
        void Log(string message, LogLevel logLevel);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
    }

}
