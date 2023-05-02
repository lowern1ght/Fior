using System.Drawing;
using Pastel;

namespace FiorConsole;

public static class Visual {
    public static async Task WriteHeaderAsync(String? stringSearch = null) {
        Console.Clear();
        await Console.Out.WriteLineAsync(Resources.ConsoleResources.ANSIHeader.Replace("#", "lowern1ght".Pastel(Color.OrangeRed)).Pastel(Color.DimGray)
            + Environment.NewLine);

        if (stringSearch is not null) {
            await Console.Out.WriteLineAsync($" *String search: {stringSearch.Pastel(Color.DimGray)}");
        }
    }

    public static async Task WriteWebsiteAsync(String? siteName, String? uri, String? status = null, WriteWebSiteConfig? webSiteConfig = null) {
        await Console.Out.WriteLineAsync(new String(webSiteConfig?.SymbolToPlace ?? '-', webSiteConfig?.SizePlace ?? 100).Pastel(Color.DimGray));
        
        if (siteName is not null)   { await Console.Out.WriteLineAsync($" [+] Name:            {siteName.Pastel(Color.Green)}"); }
        if (uri is not null)        { await Console.Out.WriteLineAsync($" [+] Website URL:     {uri.Pastel(Color.Blue)}"); }
        if (status is not null)     { await Console.Out.WriteLineAsync($" [+] Status:          {status.Pastel(Color.AliceBlue)}"); }
    }
}