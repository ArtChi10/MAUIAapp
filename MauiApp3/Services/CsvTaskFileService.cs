using System.Globalization;
using System.Text;
using MauiApp3.Models;

namespace MauiApp3.Services;

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

        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var lines = ParseCsvRows(content);

        if (lines.Count <= 1)
        {
            return [];
        }

        var result = new List<TaskItem>();

        foreach (var line in lines.Skip(1))
        {
            if (line.Count < 7)
            {
                continue;
            }

            var task = new TaskItem
            {
                Id = 0,
                Title = line[1],
                Description = line[2],
                DueDate = DateTime.TryParse(line[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dueDate)
                 ? dueDate
                    : DateTime.Today,
                IsCompleted = bool.TryParse(line[4], out var isCompleted) && isCompleted,
                Priority = Enum.TryParse<TaskPriority>(line[5], out var priority)
                    ? priority
                    : TaskPriority.Medium,
                LastModified = DateTime.TryParse(line[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var modified)
                    ? modified
                    : DateTime.UtcNow
            };

            result.Add(task);
        }

        return result;
    }

    private static string Escape(string? value)
    {
        var escaped = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static List<List<string>> ParseCsvRows(string content)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var currentCell = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                {
                    currentCell.Append('"');
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
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
                continue;
            }

            if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                {
                    i++;
                }

                currentRow.Add(currentCell.ToString());
                currentCell.Clear();

                if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                {
                    rows.Add(currentRow);
                }

                currentRow = [];
                continue;
            }

            currentCell.Append(c);
        }

        currentRow.Add(currentCell.ToString());
        if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
        {
            rows.Add(currentRow);
        }

        return rows;
    }
}