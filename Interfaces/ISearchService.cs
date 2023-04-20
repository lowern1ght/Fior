namespace FiorSearchService.Interfaces;

public interface ISearchService {
    public String? ApiKey { get; set; }
    public String URISite { get; init; }

    /// <summary>Interface searcher (google.com, ya.ru)</summary>
    /// <param name="textSearch">search text</param>
    /// <param name="matchPercentage">percentage of match with the original</param>
    /// <returns>Task</returns>
    public abstract Task Search(String textSearch, UInt16 matchPercentage = 80);
}
