using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp3.Models;
using MauiApp3.Pages;
using MauiApp3.Services;

namespace MauiApp3.ViewModels;

public class TaskListViewModel : BaseViewModel
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskFileService _taskFileService;
    private readonly List<TaskItem> _allTasks = [];
    private TaskItem? _selectedTask;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "Все";
    private string _statusMessage = string.Empty;

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

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    public ICommand LoadTasksCommand { get; }
    public ICommand SelectTaskCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }

    public TaskListViewModel(ITaskRepository taskRepository, ITaskFileService taskFileService)
    {
        Title = "Список задач";
        _taskRepository = taskRepository;
        _taskFileService = taskFileService;

        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        SelectTaskCommand = new Command<TaskItem>(async task => await SelectTaskAsync(task));
        AddTaskCommand = new Command(async () => await AddTaskAsync());
        ExportCommand = new Command(async () => await ExportAsync());
        ImportCommand = new Command(async () => await ImportAsync());
    }

    public async Task LoadTasksAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            StatusMessage = string.Empty;

            await _taskRepository.InitializeAsync();
            var loadedTasks = await _taskRepository.GetAllTasksAsync();

            foreach (var existingTask in _allTasks)
            {
                existingTask.PropertyChanged -= OnTaskPropertyChanged;
            }

            _allTasks.Clear();
            _allTasks.AddRange(loadedTasks);

            foreach (var task in _allTasks)
            {
                task.PropertyChanged += OnTaskPropertyChanged;
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    private async Task AddTaskAsync()
    {
        if (IsBusy)
        {
            return;
        }
        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            var title = await Shell.Current.DisplayPromptAsync(
               "Новая задача",
               "Введите название задачи",
               accept: "Создать",
               cancel: "Отмена",
               placeholder: "Например: Подготовить отчёт");

            if (string.IsNullOrWhiteSpace(title))
            {
                StatusMessage = "Создание задачи отменено";
                return;
            }

            var description = await Shell.Current.DisplayPromptAsync(
                "Описание задачи",
                "Введите описание",
                accept: "Сохранить",
                cancel: "Пропустить",
                placeholder: "Необязательно");

            var isCompleted = SelectedStatusFilter == "Выполненные";

            var task = new TaskItem
            {
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DueDate = DateTime.Today,
                IsCompleted = isCompleted,
                Priority = TaskPriority.Medium
            };
            await _taskRepository.SaveTaskAsync(task);
            task.PropertyChanged += OnTaskPropertyChanged;
            _allTasks.Add(task);
            ApplyFilter();

            StatusMessage = "Задача добавлена";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка создания задачи: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
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

    private async Task ExportAsync()
    {
        try
        {
            var path = await _taskFileService.ExportTasksToCsvAsync(_allTasks);
            StatusMessage = $"Экспорт выполнен: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка экспорта: {ex.Message}";
        }
    }

    private async Task ImportAsync()
    {
        try
        {
            var imported = await _taskFileService.ImportTasksFromCsvAsync();
            foreach (var task in imported)
            {
                await _taskRepository.SaveTaskAsync(task);
            }

            StatusMessage = $"Импортировано задач: {imported.Count}";
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка импорта: {ex.Message}";
        }
    }

    private async void OnTaskPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not TaskItem task)
        {
            return;
        }

        if (e.PropertyName is nameof(TaskItem.Title)
            or nameof(TaskItem.Description)
            or nameof(TaskItem.DueDate)
            or nameof(TaskItem.IsCompleted)
            or nameof(TaskItem.Priority))
        {
            try
            {
                await _taskRepository.SaveTaskAsync(task);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка автосохранения: {ex.Message}";
            }
        }
    }

    private void ApplyFilter()
    {
        var query = _allTasks.AsEnumerable();

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

        var filtered = query.OrderBy(task => task.DueDate).ToList();

        Tasks.Clear();
        foreach (var task in filtered)
        {
            Tasks.Add(task);
        }
    }
}