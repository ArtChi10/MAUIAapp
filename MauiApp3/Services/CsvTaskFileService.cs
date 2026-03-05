using System.Globalization;
using System.Text;
using MauiApp3.Models;

namespace MauiKT3.Services;

public class CsvTaskFileService : ITaskFileService
{
    private const string FileName = "tasks-export.csv";

    public async Task<string> ExportTasksToCsvAsync(IEnumerable<TaskItem> tasks)
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);

        var builder = new StringBuilder();
        builder.AppendLine("Id,Title,Description,DueDate,IsCompleted,Priority,LastModified");

        foreach (var task in tasks)
        {
            var row = string.Join(",",
                task.Id,
                Escape(task.Title),
                Escape(task.Description),
                task.DueDate.ToString("O", CultureInfo.InvariantCulture),
                task.IsCompleted,
                task.Priority,
                task.LastModified.ToString("O", CultureInfo.InvariantCulture));

            builder.AppendLine(row);
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<List<TaskItem>> ImportTasksFromCsvAsync()
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл для импорта не найден. Сначала выполните экспорт.", filePath);
        }

        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            return [];
        }

        var result = new List<TaskItem>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = ParseCsvLine(line);
            if (parts.Count < 7)
            {
                continue;
            }

            var task = new TaskItem
            {
                Id = 0,
                Title = parts[1],
                Description = parts[2],
                DueDate = DateTime.TryParse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dueDate)
                    ? dueDate
                    : DateTime.Today,
                IsCompleted = bool.TryParse(parts[4], out var isCompleted) && isCompleted,
                Priority = Enum.TryParse<TaskPriority>(parts[5], out var priority)
                    ? priority
                    : TaskPriority.Medium,
                LastModified = DateTime.TryParse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var modified)
                    ? modified
                    : DateTime.UtcNow
            };

            result.Add(task);
        }

        return result;
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        values.Add(current.ToString());
        return values;
    }
}