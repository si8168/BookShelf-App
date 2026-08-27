using System.Net.Http.Json;
using BookShelf.Models;

namespace BookShelf.Services;

public class BookApiService
{
    private readonly HttpClient _httpClient;

    public BookApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openlibrary.org/")
        };
    }

    public async Task<List<BookSearchItem>> SearchBooksAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<BookSearchItem>();

        try
        {
            // Requesting specific fields to minimize payload size
            string queryUrl = $"search.json?q={Uri.EscapeDataString(searchText)}&fields=key,title,author_name,first_publish_year,cover_i";

            var response = await _httpClient.GetFromJsonAsync<BookSearchResponse>(queryUrl);
            return response?.Books ?? new List<BookSearchItem>();
        }
        catch (Exception ex)
        {
            // Handles network failures, timeout, or invalid JSON gracefully
            System.Diagnostics.Debug.WriteLine($"API Request Error: {ex.Message}");
            return new List<BookSearchItem>();
        }
    }
}