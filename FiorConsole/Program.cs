using System.Drawing;
using FiorSearchService;
using Pastel;

namespace FiorConsole {
    internal class Program {
        private static Task InitializeLogger() {
            return Task.CompletedTask;
        }

        private static async Task WriteHeader() {
            await Console.Out.WriteLineAsync(Resources.ConsoleResources.ASCIIIArt.Pastel(Color.AliceBlue)
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

            Console.Clear();
            var service = new GoogleSearch(new() {
                ApiKey = "AIzaSyDBRc-mwzyEgSpc0fq1nWbUmKQH_ZOQimY",
                Cx = "b0edae207179a4dd3",
                ElementCount = 10,
            });

            var result = await service.GetReultAsync(answer);
            if (result is null) {
                service.Dispose();
                throw new ArgumentNullException(nameof(result));
            }
                
            var pos = await service.GetPossibleAttributesProductAsync(result);
        }
    }
}