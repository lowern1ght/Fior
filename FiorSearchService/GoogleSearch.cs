using System.Text.RegularExpressions;
using Google.Apis.CustomSearchAPI.v1;
using System.Collections.Concurrent;
using FiorSearchService.Interfaces;
using System.Text.Encodings.Web;
using Newtonsoft.Json.Linq;
using System.Text.Unicode;
using CsQuery;
using CsQuery.ExtensionMethods.Internal;
using CsQuery.ExtensionMethods;
using Google.Apis.CustomSearchAPI.v1.Data;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private HttpClient HttpClient { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";
    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };


    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        HttpClient = new HttpClient();
        CustomSearch = new(
            new Google.Apis.Services.BaseClientService.Initializer() {
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    /// <summary>Get item list before search in google service</summary>
    /// <param name="textSearch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<IList<Google.Apis.CustomSearchAPI.v1.Data.Result>?> GetReultAsync(string textSearch) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }

    public async Task<IEnumerable<PossibleAttributesProduct?>> GetPossibleAttributesProductAsync(IList<Google.Apis.CustomSearchAPI.v1.Data.Result> items) {
        foreach (var item in items) {
            var result = await CompletePossibleAttributesProductAsync(item);
        }

        throw new NotImplementedException();
    }

    private async Task<PossibleAttributesProduct?> CompletePossibleAttributesProductAsync(Google.Apis.CustomSearchAPI.v1.Data.Result item) {
        PossibleAttributesProduct possibleAttributes = new() {
            WebAddress = new Uri(item.FormattedUrl),
            SiteName = item.Title
        };

        String response = await GetResponseHtmlFromWebsiteAsync(item.Link);
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
        List<String> result = new List<String>();
        foreach (var domObject in domObjects["div"]) {
            var item = domObject.GetAttribute("itemprop");
            if (item is null)
                continue;

            if (item is String itemprop && itemprop.ToLower() == "description") {
                result.Add(domObject.Value);
            }
        }

        return result;
    }

    private async Task<String> GetResponseHtmlFromWebsiteAsync(String uriWebsite) {
        var response = await HttpClient.GetStringAsync(uriWebsite);
        if (response is null)
            throw new ArgumentNullException(nameof(response));
        return response;
    }

    private Task<IEnumerable<String>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }

    public record struct PossibleAttributesProduct {
        public Uri WebAddress { get; set; }
        public String SiteName { get; init; }
        public List<Uri> UriImages { get; set; }
        public AboutProduct AboutProduct { get; set; }
    }

    public record struct AboutProduct {
        public List<String> Description { get; set; }
        public Dictionary<String, IConvertible> Specifity { get; set; }
    }
}