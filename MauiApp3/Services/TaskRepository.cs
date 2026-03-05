using System.Collections.ObjectModel;
using MauiApp3.Models;

namespace MauiApp3.Services;

public class TaskRepository
{
    public ObservableCollection<TaskItem> Tasks { get; } =
    [
        new TaskItem
        {
            Id = 1,
            Title = "Подготовить презентацию",
            Description = "Собрать материалы и сделать слайды для защиты проекта.",
            DueDate = DateTime.Today.AddDays(1),
            IsCompleted = false,
            Priority = TaskPriority.High
        },
        new TaskItem
        {
            Id = 2,
            Title = "Купить продукты",
            Description = "Молоко, хлеб, овощи и фрукты.",
            DueDate = DateTime.Today.AddDays(2),
            IsCompleted = true,
            Priority = TaskPriority.Medium
        },
        new TaskItem
        {
            Id = 3,
            Title = "Повторить MVVM",
            Description = "Пройтись по примерам команд, навигации и биндингов.",
            DueDate = DateTime.Today.AddDays(3),
            IsCompleted = false,
            Priority = TaskPriority.Low
        }
    ];

    public Task<IReadOnlyList<TaskItem>> GetTasksAsync()
    {
        return Task.FromResult<IReadOnlyList<TaskItem>>(Tasks.ToList());
    }

    public void RemoveTask(TaskItem task)
    {
        Tasks.Remove(task);
    }
}