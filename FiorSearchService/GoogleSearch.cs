using System.Collections;
using CsQuery;
using CsQuery.ExtensionMethods;
using FiorSearchService.Entity;
using FiorSearchService.Interfaces;
using FiorSearchService.Modules;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace FiorSearchService;

public enum WebDriverType {
    FireFox,
    Chrome,
    Edge
}

public record class GoogleSearch : SearchService, IDisposable {
    private IWebDriver WebDriver { get; init; }
    private CustomSearchAPIService CustomSearch { get; init; }

    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };
    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

    private void InitializationWebDriver(WebDriverType driverType) {

    }

    /// <summary> Create component google rest api search service </summary>
    /// <param name="serviceConfig"></param>
    /// <param name="driverType"></param>
    /// <exception cref="ArgumentException"></exception>
    public GoogleSearch(SearchServiceConfig serviceConfig, WebDriverType driverType) : base(serviceConfig) {
        if (driverType == WebDriverType.FireFox) {
            // Firefox =>
            var optionFirefox = new FirefoxOptions() {
                AcceptInsecureCertificates = true,
            };

            var serviceFirefox = FirefoxDriverService.CreateDefaultService();
            serviceFirefox.SuppressInitialDiagnosticInformation = true;
            serviceFirefox.HideCommandPromptWindow = true;

            optionFirefox.AddArgument(@"--disable-gpu");
            optionFirefox.AddArgument(@"--log-level=3");
            optionFirefox.AddArgument(@"--output=/dev/null");
            optionFirefox.AddArgument(@"--disable-crash-reporter");

            WebDriver = new FirefoxDriver(serviceFirefox, optionFirefox);
        }

        if (driverType == WebDriverType.Chrome) {
            // Google Chrome =>
            var optionChrome = new ChromeOptions() {
                PageLoadStrategy = PageLoadStrategy.Default,
                AcceptInsecureCertificates = true
            };

            var serviceChrome = ChromeDriverService.CreateDefaultService();
            serviceChrome.SuppressInitialDiagnosticInformation = true;
            serviceChrome.HideCommandPromptWindow = true;

            optionChrome.AddArgument(@"--disable-gpu");
            optionChrome.AddArgument(@"--log-level=3");
            optionChrome.AddArgument(@"--output=/dev/null");
            optionChrome.AddArgument(@"--disable-extensions");
            optionChrome.AddArgument(@"--disable-crash-reporter");

            WebDriver = new ChromeDriver(serviceChrome, optionChrome);
        }

        if (driverType == WebDriverType.Edge) {
            // Edge =>
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

            WebDriver = new EdgeDriver(serviceEdge, optionEdge);
        }

        //Без WebDriver'a запускать бессмысленно...
        if (WebDriver is null) {
            throw new ArgumentException(nameof(WebDriver));
        }

        // Google API
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

        if (!Uri.TryCreate(item.Link, UriKind.RelativeOrAbsolute, out var itemLink) && itemLink is null) {
            return null;
        }

        var response = GetResponseHtmlFromWebsite(itemLink);
        CQ domObjects = new CQ(response);

        //Todo: нахождение propa и изображений
        PossibleAttributesProduct possibleAttributes = new() {
            SiteName   = item.Title,
            WebAddress = uriSite,
            
        };

        var aboutProduct = await GetAboutProductAsync(domObjects);
        if (aboutProduct is not null) {
            possibleAttributes.AboutProduct = aboutProduct.Value;
        }

        return possibleAttributes;
    }


    private async ValueTask<AboutProduct?> GetAboutProductAsync(CQ domObjects) {
        var result = new AboutProduct() { 
            Specifity = new Dictionary<string, IConvertible>(),
            Description = new List<string>()
        };

        var @div = domObjects["div"];
        foreach (var divItem in @div) {

        }

        return result;
    }

    private String? GetResponseHtmlFromWebsite(Uri uriWebsite) {
        WebDriver.Navigate().GoToUrl(uriWebsite);
        var resultPage = WebDriver.PageSource;
        return resultPage;
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        WebDriver?.Dispose();
    }
}