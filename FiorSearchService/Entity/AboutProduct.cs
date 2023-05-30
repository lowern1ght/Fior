namespace FiorSearchService.Entity;

public record struct AboutProduct {
    public IList<string> Names { get; set; }
    public IList<string> Brands { get; set; }
    public IList<string> UriImages { get; set; }
    public List<string> Description { get; set; }
    public Dictionary<string, IConvertible> Specifity { get; set; }
}
