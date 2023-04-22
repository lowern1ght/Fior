using System;
namespace FiorSearchService.Hadlers;

public static class SearchResultHadler {
    public static async Task<IEnumerable<Uri>> GetImageUri<T>(T result) where T : IEnumerable<Uri> {
        throw new NotImplementedException();
    }
}
