using Microsoft.VisualBasic;
using Serilog;

namespace FiorSearchService.Modules;

public class LogService {

    public async Task Log(Object message, LogType type, params object[] objects) {
        return;
        
        switch(type) {
            case LogType.Errored:
                Serilog.Log.Error(message as String, objects);
                break;
            case LogType.Debug:
                Serilog.Log.Debug(message as String, objects);
                break;
            case LogType.Info:
                Serilog.Log.Information(message as String, objects);
                break;
        }

        await Task.CompletedTask;
    }

    public LogService(LoggingTo loggingTo = 0)
        => Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
}
