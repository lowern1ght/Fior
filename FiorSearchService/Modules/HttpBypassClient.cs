namespace FiorSearchService.Modules;

public class HttpBypassClient {
    private HttpClient HttpClient { get; set; }

    public HttpBypassClient()
        : this(new HttpClient()) {

    }

    public HttpBypassClient(HttpClient httpClient) {
        HttpClient = httpClient;
    }

    public async Task GetBypassRequestAsync() {





    }
}
