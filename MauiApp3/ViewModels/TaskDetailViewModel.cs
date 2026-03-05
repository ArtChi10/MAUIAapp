using MauiApp3.ViewModels;
using MauiApp3.Models;
using MauiApp3.Services;
using System.Windows.Input;

namespace MauiApp3.ViewModels;

public class TaskDetailViewModel : BaseViewModel
{
    private readonly TaskRepository _taskRepository;
    private TaskItem? _currentTask;

    public TaskItem? CurrentTask
    {
        get => _currentTask;
        set
        {
            if (SetProperty(ref _currentTask, value) && value is not null)
            {
                Title = value.Title;
            }
        }
    }

    public ICommand ToggleStatusCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand BackCommand { get; }

    public TaskDetailViewModel(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
        ToggleStatusCommand = new Command(ToggleStatus);
        SaveCommand = new Command(SaveTask);
        DeleteCommand = new Command(async () => await DeleteTaskAsync());
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private void ToggleStatus()
    {
        if (CurrentTask is null)
        {
            return;
        }

        CurrentTask.IsCompleted = !CurrentTask.IsCompleted;
        OnPropertyChanged(nameof(CurrentTask));
    }

    private void SaveTask()
    {
        if (CurrentTask is null)
        {
            return;
        }

        CurrentTask.Title = CurrentTask.Title.Trim();
        CurrentTask.Description = CurrentTask.Description.Trim();
        Title = CurrentTask.Title;
        OnPropertyChanged(nameof(CurrentTask));
    }

    private async Task DeleteTaskAsync()
    {
        if (CurrentTask is null)
        {
            return;
        }

        _taskRepository.RemoveTask(CurrentTask);
        await Shell.Current.GoToAsync("..");
    }
}