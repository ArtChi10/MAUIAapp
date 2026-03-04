namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
            Routing.RegisterRoute(nameof(IncomePage), typeof(IncomePage));
            Routing.RegisterRoute(nameof(ExpensePage), typeof(ExpensePage));
            Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
        }
    }
}