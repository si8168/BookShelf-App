using BookShelf.Models;
using BookShelf.Services;

namespace BookShelf.Views;

public partial class MyBooksPage : ContentPage
{
    private readonly BookDatabaseService _databaseService;
    private List<SavedBook> _allSavedBooks = new();
    private bool _isUpdating = false;
    private const int AnnualGoal = 10;

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
        if (_isUpdating) return;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        _isUpdating = true;
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

        // 3. Sorting Logic
        filtered = SortPicker?.SelectedIndex switch
        {
            1 => filtered.OrderBy(b => b.Title),
            2 => filtered.OrderByDescending(b => b.Rating),
            _ => filtered.OrderByDescending(b => b.DateSaved)
        };

        SavedBooksCollectionView.ItemsSource = filtered.ToList();
        UpdateGoalTracker();
        _isUpdating = false;
    }

    private void UpdateGoalTracker()
    {
        int completedCount = _allSavedBooks.Count(b => b.ReadingStatus == "Completed");
        double progress = Math.Clamp((double)completedCount / AnnualGoal, 0.0, 1.0);

        GoalProgressBar.Progress = progress;
        GoalProgressLabel.Text = $"{completedCount} / {AnnualGoal} Completed";
    }

    private async void OnStatusChanged(object sender, EventArgs e)
    {
        if (_isUpdating) return;

        if (sender is Picker picker && picker.BindingContext is SavedBook book)
        {
            // TwoWay binding updates book.ReadingStatus; persist directly to SQLite
            bool updated = await _databaseService.UpdateBookAsync(book);
            if (updated)
            {
                UpdateGoalTracker();
            }
        }
    }

    private async void OnRatingChanged(object sender, EventArgs e)
    {
        if (_isUpdating) return;

        if (sender is Picker picker && picker.BindingContext is SavedBook book)
        {
            // TwoWay binding updates book.Rating; persist directly to SQLite
            await _databaseService.UpdateBookAsync(book);
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
                await _databaseService.UpdateBookAsync(book);
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