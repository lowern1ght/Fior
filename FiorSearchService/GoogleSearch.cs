namespace FiorSearchService;

using Entity;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.Services;
using HtmlAgilityPack;
using Interfaces;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

public sealed class GoogleSearch : SearchService, IDisposable 
{
    private readonly static string XPathScheme = @".//*[@itemtype='http://schema.org/Product' or @itemscope='http://schema.org/Product'] or ";
    
    private readonly static string[] ExtensionImage = {
        ".jpeg", ".png", ".jpg",
    };

    public readonly static string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

    /// <summary> Create component google rest api search service </summary>
    /// <param name="serviceConfig"></param>
    /// <param name="driverType"></param>
    /// <exception cref="ArgumentException"></exception>
    public GoogleSearch(SearchServiceConfig serviceConfig, WebDriverType driverType) 
        : base(serviceConfig)
    {

        if (driverType == WebDriverType.FireFox)
        {
            // Firefox =>
            var optionFirefox = new FirefoxOptions {
                PageLoadStrategy = PageLoadStrategy.Eager,
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

        if (driverType == WebDriverType.Chrome)
        {
            // Google Chrome =>
            var optionChrome = new ChromeOptions {
                PageLoadStrategy = PageLoadStrategy.Eager,
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
        }

        if (driverType == WebDriverType.Edge)
        {
            // Edge =>
            var optionEdge = new EdgeOptions {
                PageLoadStrategy = PageLoadStrategy.Eager,
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
        if (WebDriver is null)
        {
            throw new ArgumentException(nameof(WebDriver));
        }

        WebDriver.Manage().Timeouts().PageLoad = TimeOutWebDriver;
        WebDriver.Manage().Network.ClearAuthenticationHandlers();

        // Google API
        CustomSearch = new CustomSearchAPIService(
        new BaseClientService.Initializer {
            ApiKey = ServiceConfig.ApiKey,
        });
    }
    private IWebDriver WebDriver { get; init; }
    private CustomSearchAPIService CustomSearch { get; }

    private TimeSpan TimeOutWebDriver { get; } = TimeSpan.FromSeconds(24);

    public void Dispose()
    {
        WebDriver?.Dispose();
    }

    /// <summary>Get item list before search in google service</summary>
    /// <param name="textSearch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<IList<Result>?> GetResultAsync(string textSearch)
    {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return result.Items;
    }

    public async Task<IEnumerable<PossibleAttributesProduct?>> GetPossibleAttributesProductAsync(IList<Result> items)
    {
        var result = new List<PossibleAttributesProduct?>();

        foreach (var item in items)
        {
            var possible = await CompletePossibleAttributesProductAsync(item);

            if (possible != null) { result.Add(possible); }
        }

        return result;
    }

    private async Task<PossibleAttributesProduct?> CompletePossibleAttributesProductAsync(Result item)
    {
        if (!Uri.TryCreate(item.FormattedUrl, UriKind.RelativeOrAbsolute, out var uriSite))
        {
            return null;
        }

        if (!Uri.TryCreate(item.Link, UriKind.RelativeOrAbsolute, out var itemLink) && itemLink is null)
        {
            return null;
        }

        string siteSource = GetResponseHtmlFromWebsite(itemLink);

        if (siteSource.Length == 0)
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(siteSource);

        PossibleAttributesProduct possibleAttributes = new PossibleAttributesProduct {
            SiteName = item.Title,
            WebAddress = uriSite,
        };

        var aboutProduct = await GetAboutProductAsync(document);
        possibleAttributes.AboutProduct = aboutProduct;

        return possibleAttributes;
    }


    private async ValueTask<AboutProduct> GetAboutProductAsync(HtmlDocument document)
    {
        var result = new AboutProduct {
            Names = new List<string>(),
            UriImages = new List<string>(),
            Description = new List<string>(),
            Specifity = new Dictionary<string, IConvertible>(),
        };

        var root = document.DocumentNode;

        //Сначала проходим по Open Graphs
        var meta = root.SelectNodes(@"//meta");

        if (meta is not null)
        {
            Parallel.ForEach(meta,
            body: n => {
                if (n.GetAttributeValue("property", def: null) is {} property)
                {
                    switch (property)
                    {
                        case "og:description":
                            result.Description.Add(n.GetAttributeValue("content", string.Empty));
                            break;
                        case "og:image":
                            result.UriImages.Add(n.GetAttributeValue("content", string.Empty));
                            break;
                        case "og:title":
                            result.Names.Add(n.GetAttributeValue("content", string.Empty));
                            break;
                    }
                }
            });
        }

        var elementsDesciptions = root.SelectNodes(@"//*[@name='description' | @property='description']");

        if (elementsDesciptions is not null)
        {
            foreach (var item in elementsDesciptions)
            {
                if (item.GetAttributeValue("content", def: null) is {} cnt)
                {
                    result.Description.Add(cnt);
                }
            }
        }

        if (IsProductScheme(root))
        {
            var ds = root.SelectNodes(@"//*[@itemprop='description']");

            if (ds is not null) { AddSelectedNodes(ds, result.Description); }

            var imgs = root.SelectNodes(@"//*[@itemprop='image']");

            if (imgs is not null) { AddSelectedNodes(imgs, result.UriImages); }

            var brds = root.SelectNodes(@"//*[@itemprop='brand']");

            if (brds is not null) { AddSelectedNodes(brds, result.Brands); }
        }

        return result;
    }

    private void AddSelectedNodes(IEnumerable<HtmlNode> collection, IList<string> addCollection, string propertyName = "content")
    {
        foreach (var item in collection)
        {
            if (item.GetAttributeValue(propertyName, def: null) is {} cnt)
            {
                addCollection.Add(cnt);
            }
        }
    }

    //FIXME: убрать рутовскую ноду из проверки на схему
    private bool IsProductScheme(HtmlNode root)
    {
        HtmlNode found = root.SelectSingleNode(XPathScheme);
        return found is {};
    }

    private string GetContentForNode(HtmlNode childNode)
    {
        if (childNode.InnerText.Trim().Length != 0)
        {
            return childNode.InnerText;
        }

        if (childNode.HasAttributes)
        {
            return childNode.GetAttributeValue("content", string.Empty);
        }

        return string.Empty;
    }

    private string GetResponseHtmlFromWebsite(Uri uriWebsite)
    {
        string resultPage = string.Empty;

        try
        {
            WebDriver.Navigate().GoToUrl(uriWebsite);
            resultPage = WebDriver.PageSource;
        }

        catch (WebDriverException)
        {
            return resultPage;
        }

        catch (Exception e)
        {
            //Todo: желательно логировать данную дрянь, но у меня тестовая версия, мне можно
            Console.WriteLine(e.Message);
            throw;
        }

        return resultPage;
    }

    private Task<IEnumerable<string>> JTokenToStrings(JArray array)
    {
        throw new NotImplementedException();
    }
}
