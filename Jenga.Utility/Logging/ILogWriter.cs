using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    public interface ILogWriter
    {
        void Write(LogLevel level, string message);
    }

}
