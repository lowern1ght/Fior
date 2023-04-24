using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.CustomSearchAPI.v1;
using FiorSearchService.Interfaces;
using FiorSearchService.Entity;
using Newtonsoft.Json.Linq;
using CsQuery;
using FiorSearchService.Modules;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private HttpClient HttpClient { get; init; }
    private LogService LogService { get; init; }

    public CustomSearchAPIService CustomSearch { get; init; }

    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";
    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };


    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        HttpClient = new HttpClient();
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

        var response = await GetResponseHtmlFromWebsiteAsync(item.Link);
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
                await LogService.Log("Added description: {0}", LogType.Info, value);
                result.Add(domObject.Value);
            }
        }

        return result;
    }

    private async Task<String?> GetResponseHtmlFromWebsiteAsync(string uriWebsite) {
        //Todo: Добавить обработку под SSL и добавить защиту SSL, так как большинство сайтов не пропускает без SSL сертифката доступа

        try {
            var response = await HttpClient.GetStringAsync(uriWebsite);
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            return response;
        } catch (HttpRequestException e) {
            await LogService.Log(e.Message + @", Url: [{0}]", LogType.Errored, uriWebsite.ToString());
            return null;
        }
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }
}
