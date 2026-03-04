namespace MauiApp1;

[QueryProperty(nameof(TransactionType), "transactionType")]
public partial class AddTransactionPage : ContentPage
{
    private string _transactionType = "expense";

    public string TransactionType
    {
        get => _transactionType;
        set
        {
            _transactionType = value;
            TransactionTypeLabel.Text = value == "income" ? "Тип: доход" : "Тип: расход";
        }
    }

    public AddTransactionPage()
    {
        InitializeComponent();
        DatePicker.Date = DateTime.Today;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!decimal.TryParse(AmountEntry.Text, out var amount))
        {
            await DisplayAlert("Ошибка", "Введите корректную сумму.", "OK");
            return;
        }

        if (TransactionType != "income")
        {
            amount = -Math.Abs(amount);
        }

        var description = Uri.EscapeDataString(DescriptionEntry.Text ?? "Новая операция");
        var icon = Uri.EscapeDataString(string.IsNullOrWhiteSpace(IconEntry.Text) ? "💳" : IconEntry.Text);
        var date = Uri.EscapeDataString(DatePicker.Date.ToString("dd.MM.yyyy"));

        await Shell.Current.GoToAsync($"//MainPage?newDescription={description}&newAmount={amount}&newDate={date}&newIcon={icon}");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}