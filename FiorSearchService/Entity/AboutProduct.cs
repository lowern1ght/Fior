namespace FiorSearchService.Entity;

public record struct AboutProduct {
    public IList<String> Names { get; set; }
    public IList<String> Brands { get; set; }
    public IList<String> UriImages { get; set; }
    public List<string> Description { get; set; }
    public Dictionary<string, IConvertible> Specifity { get; set; }
}