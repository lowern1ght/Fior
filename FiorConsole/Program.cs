using Serilog;
using FiorSearchService;
using FiorSearchService.Realization;
using Pastel;

namespace FiorConsole {
    internal class Program {
        private static Task InitializeLogger() {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            return Task.CompletedTask;
        }

        private static async Task WriteHeader() {
            await Console.Out.WriteLineAsync(
                Environment.NewLine +
                Resources.ConsoleResources.ANSIHeader.Pastel(System.Drawing.Color.DimGray) 
                + Environment.NewLine);
        }

        static async Task Main() {
            await InitializeLogger();
            await WriteHeader();

            await Console.Out.WriteAsync("  Write search string: ".Pastel(ConsoleColor.Gray) 
                + Environment.NewLine + "    > ".Pastel(ConsoleColor.Green));
            String? answer = Console.ReadLine();
            if (answer is null) {
                await Console.Out.WriteLineAsync("Answer".Pastel(ConsoleColor.DarkRed) + " is empty string!");
                Environment.Exit(0);
            }

            var service = new GoogleSearch(new() {
                ApiKey = "AIzaSyDBRc-mwzyEgSpc0fq1nWbUmKQH_ZOQimY",
                Cx = "b0edae207179a4dd3",
                ElementCount = 10,
            });

            Console.Clear();
            var result = await service.SearchAsync(answer);
            if (result is not null) Parallel.ForEach(result, async d => 
                await Console.Out.WriteLineAsync($"Title: [{d.Title.Pastel(System.Drawing.Color.BlueViolet)}] [{d.Snippet.Pastel(ConsoleColor.Gray)}]"));
            else throw new ArgumentNullException(nameof(result));
        }
    }
}