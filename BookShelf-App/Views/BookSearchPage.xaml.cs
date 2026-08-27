using BookShelf.Models;
using BookShelf.Services;

namespace BookShelf.Views;

public partial class BookSearchPage : ContentPage
{
    private readonly BookApiService _apiService;
    private readonly BookDatabaseService _databaseService;

    public BookSearchPage(BookApiService apiService, BookDatabaseService databaseService)
    {
        InitializeComponent();
        _apiService = apiService;
        _databaseService = databaseService;
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string query = BookSearchBar.Text?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert("Notice", "Please enter a search term.", "OK");
            return;
        }

        // Network / Loading UI feedback
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        StatusLabel.Text = "Searching Open Library...";
        ResultsCollectionView.ItemsSource = null;

        try
        {
            var results = await _apiService.SearchBooksAsync(query);

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            if (results == null || results.Count == 0)
            {
                StatusLabel.Text = "No books found. Try another search.";
            }
            else
            {
                StatusLabel.Text = $"Found {results.Count} results:";
                ResultsCollectionView.ItemsSource = results;
            }
        }
        catch (Exception)
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            StatusLabel.Text = "Books could not be loaded. Check your connection.";
            await DisplayAlert("Error", "Books could not be loaded. Check your internet connection and try again.", "OK");
        }
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BookSearchItem selectedBook)
        {
            await Navigation.PushAsync(new BookDetailsPage(selectedBook, _databaseService));
        }
    }

    private async void OnSaveBookClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BookSearchItem item)
        {
            // Part 11 & 12: Save item & prevent duplicate saved books
            bool isAlreadySaved = await _databaseService.IsBookSavedAsync(item.Key);
            if (isAlreadySaved)
            {
                await DisplayAlert("Already Saved", "This book is already in your reading list.", "OK");
                return;
            }

            var savedBook = new SavedBook
            {
                OpenLibraryKey = item.Key,
                Title = item.Title,
                Author = item.Author,
                FirstPublishYear = item.FirstPublishYear,
                CoverId = item.CoverId,
                CoverUrl = item.CoverUrl,
                DateSaved = DateTime.Now,
                ReadingStatus = "Want to Read"
            };

            bool success = await _databaseService.SaveBookAsync(savedBook);
            if (success)
            {
                await DisplayAlert("Success", "Book added to My Books.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save book locally.", "OK");
            }
        }
    }
}