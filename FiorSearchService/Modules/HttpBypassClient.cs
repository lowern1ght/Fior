namespace FiorSearchService.Modules;

public class HttpBypassClient {

    public HttpBypassClient()
        : this(new HttpClient())
    {

    }

    public HttpBypassClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
    private HttpClient HttpClient { get; }

    public async Task GetBypassRequestAsync()
    {





    }
}
