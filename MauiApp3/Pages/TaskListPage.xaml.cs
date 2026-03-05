using MauiApp3.ViewModels;

namespace MauiApp3.Pages;

public partial class TaskListPage : ContentPage
{
    private readonly TaskListViewModel _viewModel;

    public TaskListPage(TaskListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.Tasks.Count == 0)
        {
            _viewModel.LoadTasksCommand.Execute(null);
        }
    }
}