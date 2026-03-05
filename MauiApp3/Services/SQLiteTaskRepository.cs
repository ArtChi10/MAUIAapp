
using MauiApp3.Models;
using SQLite;

namespace MauiApp3.Services;

public class SQLiteTaskRepository : ITaskRepository
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;

    public SQLiteTaskRepository()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _database.CreateTableAsync<TaskItem>();
        await _database.ExecuteAsync("CREATE TABLE IF NOT EXISTS AppMeta ([Key] TEXT PRIMARY KEY, [Value] TEXT NOT NULL)");

        await ApplyMigrationsAsync();
        await SeedDataIfEmptyAsync();

        _initialized = true;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        await InitializeAsync();
        return await _database.Table<TaskItem>().OrderBy(task => task.DueDate).ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        await InitializeAsync();
        return await _database.Table<TaskItem>().FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task SaveTaskAsync(TaskItem task)
    {
        await InitializeAsync();

        task.LastModified = DateTime.UtcNow;

        if (task.Id == 0)
        {
            await _database.InsertAsync(task);
            return;
        }

        await _database.InsertOrReplaceAsync(task);
    }

    public async Task DeleteTaskAsync(TaskItem task)
    {
        await InitializeAsync();
        await _database.DeleteAsync(task);
    }

    private async Task ApplyMigrationsAsync()
    {
        var version = await GetSchemaVersionAsync();

        if (version < 1)
        {
            await SetSchemaVersionAsync(1);
            version = 1;
        }

        if (version < 2)
        {
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.LastModified), "TEXT NOT NULL DEFAULT '2000-01-01T00:00:00.0000000Z'");
            await SetSchemaVersionAsync(2);
            version = 2;
        }
        if (version < 3)
        {
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.Description), "TEXT NOT NULL DEFAULT ''");
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.DueDate), "TEXT NOT NULL DEFAULT '2000-01-01T00:00:00.0000000Z'");
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.IsCompleted), "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.Priority), "INTEGER NOT NULL DEFAULT 1");
            await EnsureColumnAsync(nameof(TaskItem), nameof(TaskItem.LastModified), "TEXT NOT NULL DEFAULT '2000-01-01T00:00:00.0000000Z'");


            await SetSchemaVersionAsync(3);
        }
    }

    private async Task EnsureColumnAsync(string tableName, string columnName, string sqlDefinition)
    {
        var columns = await _database.GetTableInfoAsync(tableName);
        var hasColumn = columns.Any(column =>
            string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase));

        if (!hasColumn)
        {
            await _database.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlDefinition}");
        }
    }

    private async Task<int> GetSchemaVersionAsync()
    {
        var value = await _database.ExecuteScalarAsync<string>("SELECT [Value] FROM AppMeta WHERE [Key]='SchemaVersion' LIMIT 1");
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private async Task SetSchemaVersionAsync(int version)
    {
        await _database.ExecuteAsync(
            "INSERT OR REPLACE INTO AppMeta ([Key], [Value]) VALUES ('SchemaVersion', ?)",
            version.ToString());
    }

    private async Task SeedDataIfEmptyAsync()
    {
        var count = await _database.Table<TaskItem>().CountAsync();
        if (count > 0)
        {
            return;
        }

        var seed = new List<TaskItem>
        {
            new()
            {
                Title = "Подготовить презентацию",
                Description = "Собрать материалы и сделать слайды для защиты проекта.",
                DueDate = DateTime.Today.AddDays(1),
                IsCompleted = false,
                Priority = TaskPriority.High
            },
            new()
            {
                Title = "Купить продукты",
                Description = "Молоко, хлеб, овощи и фрукты.",
                DueDate = DateTime.Today.AddDays(2),
                IsCompleted = true,
                Priority = TaskPriority.Medium
            },
            new()
            {
                Title = "Повторить MVVM",
                Description = "Пройтись по примерам команд, навигации и биндингов.",
                DueDate = DateTime.Today.AddDays(3),
                IsCompleted = false,
                Priority = TaskPriority.Low
            }
        };

        foreach (var item in seed)
        {
            await SaveTaskAsync(item);
        }
    }
}