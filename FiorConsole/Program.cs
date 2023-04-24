﻿using System.Drawing;
using FiorSearchService;
using Serilog;
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
            await Console.Out.WriteLineAsync(Resources.ConsoleResources.ANSIHeader.Pastel(Color.DimGray) 
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
            var result = await service.GetReultAsync(answer);
            if (result is null)
                throw new ArgumentNullException(nameof(result));
            Parallel.ForEach(result, x => Console.WriteLine(x.HtmlTitle));

            var pos = await service.GetPossibleAttributesProductAsync(result);
            foreach (var attr in pos) {
                Console.WriteLine(attr?.SiteName);
            }
        }
    }
}