using FiorSearchService.Entity;
using FiorSearchService.Interfaces;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chromium;

namespace FiorSearchService;

public sealed class GoogleSearch : SearchService, IDisposable {
    #region //**** Variable, Const variable ****//

    private IWebDriver WebDriver { get; init; }
    private CustomSearchAPIService CustomSearch { get; init; }
    
    private TimeSpan TimeOutWebDriver { get; set; } = TimeSpan.FromSeconds(8);

    private static readonly string[] ExtensionImage = new string[] { ".jpeg", ".png", ".jpg" };
    private static readonly string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";


    #endregion

    #region //**** Constructors ****//

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

        WebDriver.Manage().Timeouts().PageLoad = TimeOutWebDriver;
        WebDriver.Manage().Network.ClearAuthenticationHandlers();

        // Google API
        CustomSearch = new(
            new Google.Apis.Services.BaseClientService.Initializer() {
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    #endregion

    #region  //**** Public methods ****//
    
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

    #region  (Interface)//--- IDisposable ---//

    public void Dispose() {
        WebDriver?.Dispose();
    }

    #endregion

    #endregion

    #region //**** Private methods ****//

    private async Task<PossibleAttributesProduct?> CompletePossibleAttributesProductAsync(Result item) {
        if (!Uri.TryCreate(item.FormattedUrl, UriKind.RelativeOrAbsolute, out var uriSite))
            return null;

        if (!Uri.TryCreate(item.Link, UriKind.RelativeOrAbsolute, out var itemLink) && itemLink is null) {
            return null;
        }

        String siteSource = GetResponseHtmlFromWebsite(itemLink);
        
        if (siteSource.Length == 0) {
            return null;
        }

        HtmlDocument @document = new HtmlDocument();
        document.LoadHtml(siteSource);

        PossibleAttributesProduct possibleAttributes = new() {
            SiteName   = item.Title,
            WebAddress = uriSite,
        };

        AboutProduct aboutProduct = await GetAboutProductAsync(document);
        possibleAttributes.AboutProduct = aboutProduct;

        return possibleAttributes;
    }


    private async ValueTask<AboutProduct> GetAboutProductAsync(HtmlDocument @document) {
        var result = new AboutProduct() {
            Specifity = new Dictionary<string, IConvertible>(),
            Description = new List<string>()
        };

        if (IsProductScheme(document.DocumentNode, out var productNode)) {

            //TODO: проверка meta части заголовка страницы



            var descriptioDivs = productNode?.SelectNodes(@".//*[@itemprop='description']")?.Descendants();
            if (descriptioDivs is not null) {
                foreach (var node in descriptioDivs) {
                    result.Description.Add(GetContentForNode(node));
                }
            }

            var nameDivs = productNode?.SelectSingleNode(@".//*[@itemprop='name']")?.Descendants();
            if (nameDivs is not null) {
                var rtn = nameDivs.First().GetAttributeValue("content", String.Empty);
                result.Name =  rtn == String.Empty ? nameDivs.First().InnerText : rtn;
            }
        }

        else {
            //Todo: not a scheme product :(
        }

        return result;
    }

    //FIXME: убрать рутовскую ноду из проверки на схему
    private Boolean IsProductScheme(HtmlNode root, out HtmlNode? finded) {
        finded = root.SelectSingleNode(@".//*[@itemtype='http://schema.org/Product' or @itemscope='http://schema.org/Product']");
        return finded is not null;
    }

    private String GetContentForNode(HtmlNode childNode) {
        if (childNode.InnerText.Trim().Length != 0) {
            return childNode.InnerText;
        }

        if (childNode.HasAttributes) {
            return childNode.GetAttributeValue("content", String.Empty);
        }

        return String.Empty;
    }

    private String GetResponseHtmlFromWebsite(Uri uriWebsite) {
        String resultPage = String.Empty;

        try {
            WebDriver.Navigate().GoToUrl(uriWebsite);
            resultPage = WebDriver.PageSource;
        }

        catch (WebDriverException) {
            return resultPage;
        }

        catch (Exception e) {
            //Todo: желательно логировать данную дрянь, но у меня тестовая версия, мне можно
            Console.WriteLine(e.Message);
            throw;
        }

        return resultPage;
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }
    
    #endregion
}