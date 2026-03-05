using MauiApp3.Models;

namespace MauiApp3.Services;

public interface ITaskFileService
{
    Task<string> ExportTasksToCsvAsync(IEnumerable<TaskItem> tasks);
    Task<List<TaskItem>> ImportTasksFromCsvAsync();
}