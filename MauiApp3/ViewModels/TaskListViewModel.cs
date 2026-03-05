using MauiApp3.Models;
using MauiApp3.Pages;
using MauiApp3.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MauiApp3.ViewModels;

public class TaskListViewModel : BaseViewModel
{
    private readonly TaskRepository _taskRepository;
    private TaskItem? _selectedTask;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "Все";

    public ObservableCollection<TaskItem> Tasks { get; } = [];
    public ObservableCollection<string> StatusFilters { get; } = ["Все", "Выполненные", "Невыполненные"];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                ApplyFilter();
            }
        }
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    public ICommand LoadTasksCommand { get; }
    public ICommand SelectTaskCommand { get; }

    public TaskListViewModel(TaskRepository taskRepository)
    {
        Title = "Список задач";
        _taskRepository = taskRepository;

        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        SelectTaskCommand = new Command<TaskItem>(async task => await SelectTaskAsync(task));

        _taskRepository.Tasks.CollectionChanged += (_, _) => ApplyFilter();
        foreach (var task in _taskRepository.Tasks)
        {
            task.PropertyChanged += (_, _) => ApplyFilter();
        }
    }

    private async Task LoadTasksAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        var loadedTasks = await _taskRepository.GetTasksAsync();
        Tasks.Clear();

        foreach (var task in loadedTasks)
        {
            if (!Tasks.Contains(task))
            {
                Tasks.Add(task);
            }
        }

        ApplyFilter();
        IsBusy = false;
    }

    private async Task SelectTaskAsync(TaskItem? task)
    {
        if (task is null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(TaskDetailPage), true, new Dictionary<string, object>
        {
            ["Task"] = task
        });

        SelectedTask = null;
    }

    private void ApplyFilter()
    {
        var query = _taskRepository.Tasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(task =>
                task.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                task.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedStatusFilter switch
        {
            "Выполненные" => query.Where(task => task.IsCompleted),
            "Невыполненные" => query.Where(task => !task.IsCompleted),
            _ => query
        };

        Tasks.Clear();
        foreach (var task in query.OrderBy(task => task.DueDate))
        {
            Tasks.Add(task);
        }
    }
}