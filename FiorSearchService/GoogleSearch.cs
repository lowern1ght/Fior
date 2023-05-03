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

public record class GoogleSearch : SearchService, IDisposable {
    private LogService LogService { get; init; }
    private IWebDriver WebDriver { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    private readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };
    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

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
                        optionFirefox.AddArgument(@"--log-level=3");
                        optionFirefox.AddArgument(@"--output=/dev/null");
                        optionFirefox.AddArgument(@"--disable-crash-reporter");

                        WebDriver = new FirefoxDriver(serviceFirefox, optionFirefox);
                        break;
                    }
                case PlatformID.Win32NT: {
                        var optionChrome = new ChromeOptions() {
                            AcceptInsecureCertificates = true,
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
    public override async Task<IList<Result>?> GetResultAsync(string textSearch) {
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

        var response = await GetResponseHtmlFromWebsite(itemLink);
        CQ domObjects = new CQ(response);

        //Todo: нахождение propa и изображений
        PossibleAttributesProduct possibleAttributes = new() {
            SiteName   = item.Title,
            WebAddress = uriSite,
            AboutProduct = new AboutProduct() {
                Description = await GetDescriptionProductAsync(domObjects),
                Specifity   = await GetSpecifityProductAsync(domObjects)
            }
        };


        return possibleAttributes;
    }

    private async Task<List<String>> GetDescriptionProductAsync(CQ domObjects) {
        var description = new List<String>();

        return description;
    }

    private async Task<Dictionary<String, IConvertible>> GetSpecifityProductAsync(CQ domObjects) {
        var specifity = new Dictionary<String, IConvertible>();

        //find atribute itemtype
        var @div = domObjects["div"];
        foreach (var itemDiv in @div) {

        }

        return specifity;
    }

    private async Task<String?> GetResponseHtmlFromWebsite(Uri uriWebsite) {
        try {
            WebDriver.Navigate().GoToUrl(uriWebsite);
            var resultPage = WebDriver.PageSource;
            await LogService.Log("WebSite: {0}, Status: {Open}", Modules.LogType.Info, uriWebsite.ToString(), "Open");
            return resultPage;
        }
        catch (WebDriverException ex) {
            await LogService.Log(ex.Message, Modules.LogType.Errored, ex);
            return null;
        }
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        WebDriver?.Dispose();
    }
}