using System.Drawing;
using Pastel;

namespace FiorConsole;

public static class Visual {
    public static async Task WriteHeaderAsync(String? stringSearch = null) {
        Console.Clear();
        await Console.Out.WriteLineAsync(Resources.ConsoleResources.ANSIHeader.Replace("#", "lowern1ght".Pastel(Color.Bisque)).Pastel(Color.DimGray)
        + Environment.NewLine);

        if (stringSearch is not null) {
            await Console.Out.WriteLineAsync($" String search: {stringSearch.Pastel(Color.DimGray)}*");
        }
    }

    public static async Task WriteWebsiteAsync(String siteName, String uri, String? status = null, WriteWebSiteConfig? webSiteConfig = null) {
        await Console.Out.WriteLineAsync(new String(webSiteConfig?.SymbolToPlace ?? '—', webSiteConfig?.SizePlace ?? 100).Pastel(Color.DimGray));
        await Console.Out.WriteLineAsync($" [+] Name:            {siteName.Pastel(Color.LawnGreen)}");
        await Console.Out.WriteLineAsync($" [+] Website URL:     {uri.Pastel(Color.DeepSkyBlue)}");

        if (status is not null) { 
            await Console.Out.WriteLineAsync($" [+] Status:          {status.Pastel(Color.AliceBlue)}");
        }
    }

    public struct WriteWebSiteConfig {
        public UInt16 SizePlace { get; set; }
        public Char SymbolToPlace { get; set; }
    }
}