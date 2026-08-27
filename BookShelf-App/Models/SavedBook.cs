using SQLite;

namespace BookShelf.Models;

[Table("SavedBooks")]
public class SavedBook
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string OpenLibraryKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public int? FirstPublishYear { get; set; }

    public long? CoverId { get; set; }

    public string CoverUrl { get; set; } = string.Empty;

    public DateTime DateSaved { get; set; } = DateTime.Now;

    public string ReadingStatus { get; set; } = "Want to Read";

    // --- Extension Features: Personal Notes & Rating ---
    public int Rating { get; set; } = 0; // 0 to 5 Stars

    public string PersonalNotes { get; set; } = string.Empty;
}