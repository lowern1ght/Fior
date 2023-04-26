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
using System.Runtime.InteropServices;
using OpenQA.Selenium.Firefox;
using CsQuery.ExtensionMethods;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private LogService LogService { get; init; }
    private IWebDriver WebDriver { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };
    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

    ~GoogleSearch() {
        WebDriver.Quit();
        WebDriver.Dispose();
    }

    public GoogleSearch(SearchServiceConfig serviceConfig, IWebDriver? webDriver = null) : base(serviceConfig) {
        LogService = new LogService(LoggingTo.Console);

        if (webDriver is null) {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix: {
                        var optionFirefox = new FirefoxOptions() {
                            AcceptInsecureCertificates = true,
                        };

                        var serviceFirefox = FirefoxDriverService.CreateDefaultService();
                        serviceFirefox.SuppressInitialDiagnosticInformation = true;
                        serviceFirefox.HideCommandPromptWindow = true;

                        optionFirefox.AddArgument(@"--disable-gpu");
                        optionFirefox.AddArgument(@"--log-level=3" );
                        optionFirefox.AddArgument(@"--output=/dev/null");
                        optionFirefox.AddArgument(@"--disable-crash-reporter");

                        WebDriver = new FirefoxDriver(serviceFirefox, optionFirefox);
                        break;
                    }
                case PlatformID.Win32NT: {
                        var optionEdge = new EdgeOptions() {
                            AcceptInsecureCertificates = true,
                        };

                        var serviceEdge = EdgeDriverService.CreateDefaultService();
                        serviceEdge.SuppressInitialDiagnosticInformation = true;
                        serviceEdge.HideCommandPromptWindow = true;
                        serviceEdge.UseVerboseLogging = false;

                        optionEdge.AddArgument(@"--disable-gpu");
                        optionEdge.AddArgument(@"--log-level=3");
                        optionEdge.AddArgument(@"--output=/dev/null");
                        optionEdge.AddArgument(@"--disable-extensions");
                        optionEdge.AddArgument(@"--disable-crash-reporter");

                        WebDriver = new EdgeDriver(serviceEdge,  optionEdge);
                        break;
                    }
                default: {
                        throw new NotSupportedException();
                    }
            }
        }
        else { WebDriver = webDriver; }

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

    public async Task AddRecursedValueAsync(IDomObject @object, IList<String> text) {
        if (@object.HasChildren) {
            foreach (var node in @object.ChildNodes) {
                await AddRecursedValueAsync(node, text);
            }
        } else {
            text.Add(@object.Value);
        }
    }

    private async Task<List<String>> GetInvariantsDescriptionsAsync(CQ domObjects) {
        List<string> result = new List<string>();
        foreach (var domObject in domObjects["div"]) {
            var item = domObject.GetAttribute("itemprop");
            if (item is null)
                continue;

            if (item is string itemprop && itemprop.ToLower() == "description") {
                switch (itemprop.ToLower()) {
                    case "description": {
                            await AddRecursedValueAsync(domObject, result);
                            break;
                        }
                }





                var value = domObject.Value;
                await LogService.Log("Added description: {0}", Modules.LogType.Info, value);
                result.Add(domObject.Value);
            } else {
                continue;
            }
        }

        //foreach (var domObject in domObjects["t"])

        return result;
    }

    private async Task<String?> GetResponseHtmlFromWebsite(Uri uriWebsite) {
        try {
            WebDriver.Navigate().GoToUrl(uriWebsite);
            await LogService.Log("WebSite: {0}, Status: {Open}", Modules.LogType.Info, uriWebsite.ToString(), "Open");
            return WebDriver.PageSource;
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