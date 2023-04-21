using Serilog;
using FiorSearchService;
using FiorSearchService.Realization;

namespace FiorConsole {
    internal class Program {
        private static Task InitializeLogger() {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            return Task.CompletedTask;
        }

        static async Task Main() {
            await InitializeLogger();
            var service = new GoogleSearch(new() { 
                ApiKey = "AIzaSyDBRc-mwzyEgSpc0fq1nWbUmKQH_ZOQimY",
                Cx = "b0edae207179a4dd3",
                ElementCount = 10,
            });

            var result = await service.SearchAsync("Лампа SWEKO 20W");
            if (result is not null) Parallel.ForEach(result, d => Log.Information("Title: [{0}] [{1}]",
                d.Title, d.Snippet));
            else throw new ArgumentNullException(nameof(result));
        }
    }
}