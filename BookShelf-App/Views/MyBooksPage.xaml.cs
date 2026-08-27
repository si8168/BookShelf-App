using BookShelf.Models;
using BookShelf.Services;

namespace BookShelf.Views;

public partial class MyBooksPage : ContentPage
{
    private readonly BookDatabaseService _databaseService;
    private List<SavedBook> _allSavedBooks = new();

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
        _allSavedBooks = await _databaseService.GetSavedBooksAsync();
        ApplyFilters();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allSavedBooks.AsEnumerable();

        // 1. Text Search Filter
        string searchText = SavedSearchBar.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(b => b.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                          b.Author.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        // 2. Reading Status Filter
        if (StatusFilterPicker.SelectedIndex > 0)
        {
            string selectedStatus = StatusFilterPicker.SelectedItem.ToString()!;
            filtered = filtered.Where(b => b.ReadingStatus == selectedStatus);
        }

        SavedBooksCollectionView.ItemsSource = filtered.ToList();
    }

    private async void OnStatusChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.BindingContext is SavedBook book)
        {
            book.ReadingStatus = picker.SelectedItem?.ToString() ?? "Want to Read";
            await _databaseService.SaveBookAsync(book);
        }
    }

    private async void OnRatingChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.BindingContext is SavedBook book)
        {
            if (picker.SelectedItem is int rating)
            {
                book.Rating = rating;
                await _databaseService.SaveBookAsync(book);
            }
        }
    }

    private async void OnEditNotesClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SavedBook book)
        {
            string result = await DisplayPromptAsync("Personal Notes",
                                                    $"Add notes for \"{book.Title}\":",
                                                    initialValue: book.PersonalNotes,
                                                    maxLength: 300);

            if (result != null)
            {
                book.PersonalNotes = result;
                await _databaseService.SaveBookAsync(book);
                await DisplayAlert("Saved", "Personal notes updated successfully.", "OK");
            }
        }
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SavedBook book)
        {
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