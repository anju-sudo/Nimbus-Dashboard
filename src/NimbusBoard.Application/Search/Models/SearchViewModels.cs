namespace NimbusBoard.Application.Search.Models;

public class SearchResultsViewModel
{
    public string Term { get; set; } = string.Empty;
    public IReadOnlyList<SearchHitViewModel> Issues { get; set; } = [];
    public IReadOnlyList<SearchHitViewModel> Projects { get; set; } = [];
    public IReadOnlyList<SearchHitViewModel> Boards { get; set; } = [];
}

public class SearchHitViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
