using MauiApp3.Models;

namespace MauiApp3.Services;

public interface ITaskRepository
{
    Task InitializeAsync();
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task SaveTaskAsync(TaskItem task);
    Task DeleteTaskAsync(TaskItem task);
}