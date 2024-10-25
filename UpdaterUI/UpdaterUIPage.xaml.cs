using System.Windows.Controls;
using ActivityTrackerViewModel;
using UpdaterViewModel;
using Updater;
using System.Windows;

namespace UpdaterUI;

/// <summary>
/// Interaction logic for MainPage.xaml
/// </summary>
public partial class UpdaterUIPage : Page
{
    public MainViewModel ViewModel { get; }
    private FileChangeNotifier _analyzerNotificationService;
    private ToolListViewModel _toolListViewModel;

    public UpdaterUIPage()
    {
        InitializeComponent();
        _toolListViewModel = new ToolListViewModel();
        _toolListViewModel.LoadAvailableTools();
        ListView listView = (ListView)this.FindName("ToolViewList");
        listView.DataContext = _toolListViewModel;

        _analyzerNotificationService = new FileChangeNotifier();
        _analyzerNotificationService.MessageReceived += OnMessageReceived;

        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    private void OnMessageReceived(string message)
    {
        _toolListViewModel.LoadAvailableTools();
        ViewModel.ShowNotification(message);
        ViewModel.UpdateLogDetails(message);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _toolListViewModel.LoadAvailableTools();
        ViewModel.UpdateLogDetails("Page loaded successfully.\n");
        ViewModel.ShowNotification("Welcome to the application!");
    }
}
