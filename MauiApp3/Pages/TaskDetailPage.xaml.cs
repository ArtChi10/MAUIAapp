using MauiApp3.Models;
using MauiApp3.ViewModels;

namespace MauiApp3.Pages;

public partial class TaskDetailPage : ContentPage, IQueryAttributable
{
    private readonly TaskDetailViewModel _viewModel;

    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Task", out var task) && task is TaskItem taskItem)
        {
            _viewModel.CurrentTask = taskItem;
        }
    }
}