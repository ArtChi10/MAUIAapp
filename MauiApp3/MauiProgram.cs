using Microsoft.Extensions.Logging;
using MauiApp3.Pages;
using MauiApp3.Services;
using MauiApp3.ViewModels;

namespace MauiApp3
{
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

            builder.Services.AddSingleton<ITaskRepository, SQLiteTaskRepository>();
            builder.Services.AddSingleton<ITaskFileService, CsvTaskFileService>();
            builder.Services.AddSingleton<TaskListViewModel>();
            builder.Services.AddTransient<TaskDetailViewModel>();
            builder.Services.AddSingleton<TaskListPage>();
            builder.Services.AddTransient<TaskDetailPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}