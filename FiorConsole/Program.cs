using System.Drawing;
using FiorSearchService;
using Newtonsoft.Json;
using Pastel;

namespace FiorConsole {
    internal class Program {
        private static Task InitializeLogger() {
            return Task.CompletedTask;
        }

        private static async Task<String> GetPromiseAsync(String promise) {
            await Console.Out.WriteAsync(promise.Pastel(ConsoleColor.Gray)
                + Environment.NewLine + "    > ".Pastel(ConsoleColor.Green));
            return Console.ReadLine() ?? " ";
        }

        private static async Task Main(string[] args) {
            await Visual.WriteHeaderAsync();

            String answer = await GetPromiseAsync("  Write search string: ");
            var service = new GoogleSearch(new() {
                ApiKey = "AIzaSyDBRc-mwzyEgSpc0fq1nWbUmKQH_ZOQimY",
                Cx = "b0edae207179a4dd3",
                ElementCount = 10,
            }, WebDriverType.Chrome);

            var result = await service.GetResultAsync(answer);
            if (result is null) {
                service.Dispose();
                throw new ArgumentNullException(nameof(result));
            }

            await Visual.WriteHeaderAsync(answer);
            foreach (var item in result) {
                await Visual.WriteWebsiteAsync(item.Title, item.Link);
            }

            var pos = await service.GetPossibleAttributesProductAsync(result);

            Console.Clear();
            foreach (var item in pos) {
                await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(item) + Environment.NewLine);
            }

            service.Dispose();
        }
    }
}