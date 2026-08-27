using BookShelf.Models;
using BookShelf.Services;

namespace BookShelf.Views;

public partial class MyBooksPage : ContentPage
{
    private readonly BookDatabaseService _databaseService;

    public MyBooksPage(BookDatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSavedBooksAsync();
    }

    private async Task LoadSavedBooksAsync()
    {
        var books = await _databaseService.GetSavedBooksAsync();
        SavedBooksCollectionView.ItemsSource = books;
    }

    private async void OnStatusChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.BindingContext is SavedBook book)
        {
            book.ReadingStatus = picker.SelectedItem?.ToString() ?? "Want to Read";
            await _databaseService.UpdateBookAsync(book);
        }
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SavedBook book)
        {
            // Part 15: Deletion confirmation
            bool confirm = await DisplayAlert("Confirm Delete", $"Remove \"{book.Title}\" from your reading list?", "Yes", "No");
            if (confirm)
            {
                bool deleted = await _databaseService.DeleteBookAsync(book);
                if (deleted)
                {
                    await LoadSavedBooksAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Could not remove the book from database.", "OK");
                }
            }
        }
    }
}