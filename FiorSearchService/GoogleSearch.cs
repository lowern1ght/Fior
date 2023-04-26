using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.CustomSearchAPI.v1;
using FiorSearchService.Interfaces;
using FiorSearchService.Modules;
using FiorSearchService.Entity;
using Newtonsoft.Json.Linq;
using CsQuery;

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private LogService LogService { get; init; }
    private EdgeDriver EdgeDriver { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };
    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        this.EdgeDriver = new EdgeDriver() { };
        LogService = new LogService(LoggingTo.Console);
        CustomSearch = new(
            new Google.Apis.Services.BaseClientService.Initializer() {
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    /// <summary>Get item list before search in google service</summary>
    /// <param name="textSearch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<IList<Result>?> GetReultAsync(string textSearch) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }

    public async Task<IEnumerable<PossibleAttributesProduct?>> GetPossibleAttributesProductAsync(IList<Result> items) {
        var result = new List<PossibleAttributesProduct?>();
        foreach (var item in items) {
            var possible = await CompletePossibleAttributesProductAsync(item);
            if (possible != null) { result.Add(possible); }
        }

        return result;
    }

    private async Task<PossibleAttributesProduct?> CompletePossibleAttributesProductAsync(Result item) {
        if (!Uri.TryCreate(item.FormattedUrl, UriKind.RelativeOrAbsolute, out var uriSite))
            return null;
        
        PossibleAttributesProduct possibleAttributes = new() {
            SiteName = item.Title,
            WebAddress = uriSite
        };

        if (!Uri.TryCreate(item.Link, UriKind.RelativeOrAbsolute, out var itemLink) && itemLink is null) {
            return null;
        }

        var response = await GetResponseHtmlFromWebsite(itemLink);
        CQ domObjects = new CQ(response);

        var aboutProduct = new AboutProduct() {
            Description = await GetInvariantsDescriptionsAsync(domObjects),
            Specifity = new Dictionary<string, IConvertible>()
        };

        //Images
        possibleAttributes.UriImages = domObjects["img"]
            .Select((d, i) => d.GetAttribute("src"))
            .Select(s => Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uriResult) == true ? uriResult : new Uri(""))
            .Select(u => u.IsAbsoluteUri == false ? new Uri(possibleAttributes.WebAddress, u) : u)
            .Where(u => ExtensionImage.Any(s => u.AbsolutePath.Contains(s)))
            .ToList();

        return possibleAttributes;
    }

    private async Task<List<String>> GetInvariantsDescriptionsAsync(CQ domObjects) {
        List<string> result = new List<string>();
        foreach (var domObject in domObjects["div"]) {
            var item = domObject.GetAttribute("itemprop");
            if (item is null)
                continue;

            if (item is string itemprop && itemprop.ToLower() == "description") {
                var value = domObject.Value;
                await LogService.Log("Added description: {0}", Modules.LogType.Info, value);
                result.Add(domObject.Value);
            }
        }

        return result;
    }

    private async Task<String?> GetResponseHtmlFromWebsite(Uri uriWebsite) {
        try {
            EdgeDriver.Navigate().GoToUrl(uriWebsite);
            return EdgeDriver.PageSource;
        }
        catch (WebDriverException ex) {
            await LogService.Log(ex.Message, Modules.LogType.Errored, ex);
            return null;
        }
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }
}