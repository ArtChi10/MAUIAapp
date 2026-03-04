using System.Collections.ObjectModel;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Transaction> Transactions { get; } =
    [
        new Transaction { CategoryIcon = "💼", Description = "Зарплата", Amount = 80000, Date = "20.01.2026" },
        new Transaction { CategoryIcon = "🛒", Description = "Продукты", Amount = -5400, Date = "19.01.2026" },
        new Transaction { CategoryIcon = "🚕", Description = "Такси", Amount = -750, Date = "19.01.2026" },
        new Transaction { CategoryIcon = "🎬", Description = "Кино", Amount = -1200, Date = "18.01.2026" },
        new Transaction { CategoryIcon = "💡", Description = "Коммунальные услуги", Amount = -4300, Date = "17.01.2026" },
        new Transaction { CategoryIcon = "💸", Description = "Фриланс", Amount = 12000, Date = "16.01.2026" }
    ];
    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnIncomeButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(IncomePage));
    }

    private async void OnExpenseButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ExpensePage));
    }

    private async void OnReportButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ReportPage));
    }

}

