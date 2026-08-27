using System.Text.Json.Serialization;

namespace BookShelf.Models;

public class BookSearchResponse
{
    [JsonPropertyName("docs")]
    public List<BookSearchItem>? Books { get; set; }

    [JsonPropertyName("numFound")]
    public int NumFound { get; set; }
}

public class BookSearchItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Unknown Title";

    [JsonPropertyName("author_name")]
    public List<string>? AuthorNames { get; set; }

    // Helper property to display authors cleanly in the UI
    public string Author => AuthorNames != null && AuthorNames.Count > 0
        ? string.Join(", ", AuthorNames)
        : "Unknown Author";

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    public string DisplayYear => FirstPublishYear.HasValue
        ? FirstPublishYear.Value.ToString()
        : "Year N/A";

    [JsonPropertyName("cover_i")]
    public long? CoverId { get; set; }

    // Helper property to safely construct cover image URLs
    public string CoverUrl => CoverId.HasValue
        ? $"https://covers.openlibrary.org/b/id/{CoverId.Value}-M.jpg"
        : "https://via.placeholder.com/150x200?text=No+Cover";
}