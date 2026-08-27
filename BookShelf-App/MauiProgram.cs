using BookShelf.Services;
using BookShelf.Views;
using Microsoft.Extensions.Logging;

namespace BookShelf;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<BookApiService>();
        builder.Services.AddSingleton<BookDatabaseService>();

        // Views
        builder.Services.AddTransient<BookSearchPage>();
        builder.Services.AddTransient<BookDetailsPage>();
        builder.Services.AddTransient<MyBooksPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}