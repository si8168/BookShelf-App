using SQLite;
using BookShelf.Models;

namespace BookShelf.Services;

public class BookDatabaseService
{
    private SQLiteAsyncConnection _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookshelf_v3.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<SavedBook>();
    }

    public async Task<List<SavedBook>> GetSavedBooksAsync()
    {
        await InitAsync();
        try
        {
            return await _database.Table<SavedBook>().OrderByDescending(b => b.DateSaved).ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database Get Error: {ex.Message}");
            return new List<SavedBook>();
        }
    }

    public async Task<bool> IsBookSavedAsync(string openLibraryKey)
    {
        await InitAsync();
        if (string.IsNullOrEmpty(openLibraryKey)) return false;

        var existing = await _database.Table<SavedBook>()
                                      .Where(b => b.OpenLibraryKey == openLibraryKey)
                                      .FirstOrDefaultAsync();
        return existing != null;
    }

    public async Task<bool> SaveBookAsync(SavedBook book)
    {
        await InitAsync();
        try
        {
            if (await IsBookSavedAsync(book.OpenLibraryKey))
                return false; // Prevent duplicates

            await _database.InsertAsync(book);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database Insert Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateBookAsync(SavedBook book)
    {
        await InitAsync();
        try
        {
            await _database.UpdateAsync(book);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database Update Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteBookAsync(SavedBook book)
    {
        await InitAsync();
        try
        {
            await _database.DeleteAsync(book);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database Delete Error: {ex.Message}");
            return false;
        }
    }
}