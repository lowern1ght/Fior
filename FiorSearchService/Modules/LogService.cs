namespace FiorSearchService.Modules;

using Serilog;

public class LogService {
    public LogService(LoggingTo loggingTo = 0)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }
}
