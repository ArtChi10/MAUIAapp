using MauiApp3.Pages;

namespace MauiApp3
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
        }
    }
}