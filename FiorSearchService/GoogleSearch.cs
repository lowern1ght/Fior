using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.CustomSearchAPI.v1;
using FiorSearchService.Interfaces;
using FiorSearchService.Modules;
using FiorSearchService.Entity;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net;
using CsQuery;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private HttpClient HttpClient { get; init; }
    private HttpClientHandler HttpClientHandler { get; init; }

    private LogService LogService { get; init; }

    public CustomSearchAPIService CustomSearch { get; init; }

    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";
    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };


    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        HttpClient = new HttpClient() {
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
            Timeout = new TimeSpan(9000)
        };

        HttpClientHandler = new HttpClientHandler() {
            CookieContainer = new CookieContainer()
        };

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
        PossibleAttributesProduct possibleAttributes = new() {
            WebAddress = new Uri(item.FormattedUrl),
            SiteName = item.Title
        };

        if (!Uri.TryCreate(item.Link, UriKind.RelativeOrAbsolute, out var itemLink) && itemLink is null) {
            return null;
        }

        var response = await GetResponseHtmlFromWebsiteAsync(itemLink);
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
                result.Add(domObject.Value);
            }
        }

        return result;
    }

    private async Task<String?> GetResponseHtmlFromWebsiteAsync(Uri uriWebsite) {
        try {
            var request = new HttpRequestMessage(HttpMethod.Get, uriWebsite) { };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            request.Headers.Add("User-Agent",
                @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59");

            var responseMessage = await HttpClient.SendAsync(request);
            if (!responseMessage.IsSuccessStatusCode && 
                HttpClientHandler.CookieContainer.GetCookies(uriWebsite).Count > 0 ) {

                responseMessage = await HttpClient.SendAsync(request);
                if (responseMessage.IsSuccessStatusCode) {
                    return await responseMessage.Content.ReadAsStringAsync();
                } else {
                    await LogService.Log("Response not successed", LogType.Errored);
                    return null;
                }
            }

            return await responseMessage.Content.ReadAsStringAsync();
        } 
        catch (HttpRequestException e) {
            await LogService.Log(e.Message, LogType.Errored);
            return null;
        }
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }
}
