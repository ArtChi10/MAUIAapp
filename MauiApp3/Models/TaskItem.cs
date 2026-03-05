using SQLite;
using System.ComponentModel;
using SqliteMaxLength = SQLite.MaxLengthAttribute;
using System.Runtime.CompilerServices;

namespace MauiApp3.Models;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public class TaskItem : INotifyPropertyChanged
{
    private int _id;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private DateTime _dueDate = DateTime.Today;
    private bool _isCompleted;
    private TaskPriority _priority;
    private DateTime _lastModified = DateTime.UtcNow;

    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [MaxLength(200)]
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    [MaxLength(2000)]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public DateTime DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    public TaskPriority Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    public DateTime LastModified
    {
        get => _lastModified;
        set => SetProperty(ref _lastModified, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}