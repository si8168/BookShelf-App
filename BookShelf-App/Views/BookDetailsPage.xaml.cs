using BookShelf.Models;
using BookShelf.Services;

namespace BookShelf.Views;

public partial class BookDetailsPage : ContentPage
{
    private readonly BookSearchItem _book;
    private readonly BookDatabaseService _databaseService;

    public BookDetailsPage(BookSearchItem book, BookDatabaseService databaseService)
    {
        InitializeComponent();
        _book = book;
        _databaseService = databaseService;

        PopulateDetails();
    }

    private void PopulateDetails()
    {
        CoverImage.Source = _book.CoverUrl;
        TitleLabel.Text = _book.Title;
        AuthorLabel.Text = $"Author: {_book.Author}";
        YearLabel.Text = $"First Published: {_book.DisplayYear}";
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        bool isAlreadySaved = await _databaseService.IsBookSavedAsync(_book.Key);
        if (isAlreadySaved)
        {
            await DisplayAlert("Already Saved", "This book is already in your reading list.", "OK");
            return;
        }

        var savedBook = new SavedBook
        {
            OpenLibraryKey = _book.Key,
            Title = _book.Title,
            Author = _book.Author,
            FirstPublishYear = _book.FirstPublishYear,
            CoverId = _book.CoverId,
            CoverUrl = _book.CoverUrl,
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