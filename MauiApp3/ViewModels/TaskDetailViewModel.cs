using System.ComponentModel;
using System.Windows.Input;
using MauiApp3.Models;
using MauiApp3.Services;

namespace MauiApp3.ViewModels;

public class TaskDetailViewModel : BaseViewModel
{
    private readonly ITaskRepository _taskRepository;
    private TaskItem? _currentTask;
    private string _statusMessage = string.Empty;

    public TaskItem? CurrentTask
    {
        get => _currentTask;
        set
        {
            var previousTask = _currentTask;
            if (!SetProperty(ref _currentTask, value))
            {
                return;
            }

            if (previousTask is not null)
            {
                previousTask.PropertyChanged -= OnCurrentTaskPropertyChanged;
            }

            if (_currentTask is not null)
            {
                _currentTask.PropertyChanged += OnCurrentTaskPropertyChanged;
                Title = _currentTask.Title;
            }
        }
    }


    public IReadOnlyList<TaskPriority> PriorityOptions { get; } = Enum.GetValues<TaskPriority>();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ToggleStatusCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand BackCommand { get; }

    public TaskDetailViewModel(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
        ToggleStatusCommand = new Command(async () => await ToggleStatusAsync());
        SaveCommand = new Command(async () => await SaveTaskAsync());
        DeleteCommand = new Command(async () => await DeleteTaskAsync());
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task ToggleStatusAsync()
    {
        if (CurrentTask is null)
        {
            return;
        }

        CurrentTask.IsCompleted = !CurrentTask.IsCompleted;
        await SaveTaskAsync();
    }

    private async Task SaveTaskAsync()
    {
        if (CurrentTask is null)
        {
            return;
        }

        try
        {
            CurrentTask.Title = CurrentTask.Title.Trim();
            CurrentTask.Description = CurrentTask.Description.Trim();
            Title = CurrentTask.Title;

            await _taskRepository.SaveTaskAsync(CurrentTask);
            StatusMessage = "Изменения сохранены";
            OnPropertyChanged(nameof(CurrentTask));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private async Task DeleteTaskAsync()
    {
        if (CurrentTask is null)
        {
            return;
        }

        try
        {
            await _taskRepository.DeleteTaskAsync(CurrentTask);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    private async void OnCurrentTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (CurrentTask is null)
        {
            return;
        }

        if (e.PropertyName is nameof(TaskItem.Title)
            or nameof(TaskItem.Description)
            or nameof(TaskItem.DueDate)
            or nameof(TaskItem.Priority)
            or nameof(TaskItem.IsCompleted))
        {
            await SaveTaskAsync();
        }
    }
}