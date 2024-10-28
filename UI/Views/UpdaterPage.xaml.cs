/******************************************************************************
 * Filename    = UpdaterPage.xaml.cs
 *
 * Author      = Updater Team
 *
 * Product     = UI
 * 
 * Project     = Views
 *
 * Description = Initialize a page for Updater
 *****************************************************************************/

using System.Windows;
using System.Windows.Controls;
using ViewModels;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for UpdaterPage.xaml
    /// </summary>
    public partial class UpdaterPage : Page
    {
        public LogServiceViewModel ViewModel { get; }
        private FileChangeNotifier _analyzerNotificationService;
        private ToolListViewModel _toolListViewModel;

        public UpdaterPage()
        {

            InitializeComponent();
            _toolListViewModel = new ToolListViewModel();
            _toolListViewModel.LoadAvailableTools();
            ListView listView = (ListView)this.FindName("ToolViewList");
            listView.DataContext = _toolListViewModel;

            _analyzerNotificationService = new FileChangeNotifier();
            _analyzerNotificationService.MessageReceived += OnMessageReceived;

            ViewModel = new LogServiceViewModel();
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
}
