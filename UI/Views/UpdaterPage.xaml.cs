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
        private readonly Server _server;
        public LogServiceViewModel LogServiceViewModel { get; }
        private readonly FileChangeNotifier _toolsNotificationService;
        private readonly ToolListViewModel _toolListViewModel;
        private readonly ServerViewModel _serverViewModel;
        private readonly ClientViewModel _clientViewModel; 
        public UpdaterPage()
        {
            InitializeComponent();
            _toolListViewModel = new ToolListViewModel();
            _toolListViewModel.LoadAvailableTools();

            ListView listView = (ListView)this.FindName("ToolViewList");
            listView.DataContext = _toolListViewModel;

            LogServiceViewModel = new LogServiceViewModel();
            DataContext = LogServiceViewModel;

            _serverViewModel = new ServerViewModel(LogServiceViewModel); 
            _clientViewModel = new ClientViewModel(LogServiceViewModel);
            _toolsNotificationService = new FileChangeNotifier(LogServiceViewModel, _toolListViewModel);

        }
        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the server can be started 
            if (_serverViewModel.CanStartServer())
            {
                // string ip = "10.32.2.232"; 
                string ip = "10.128.4.178"; 
                string port = "60091";

                _serverViewModel.StartServer(ip, port);

                StartServerButton.IsEnabled = false; 
                StopServerButton.IsEnabled = true; 

                DisconnectButton.IsEnabled = false; 
                ConnectButton.IsEnabled = false; 
            }
            else
            {
                LogServiceViewModel.UpdateLogDetails("Server is already running on another instance.\n");
            }
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e) 
        {
            _serverViewModel.StopServer();

            StartServerButton.IsEnabled = true;
            StopServerButton.IsEnabled = false; 
            DisconnectButton.IsEnabled = true; 
            ConnectButton.IsEnabled = true;

        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e) 
        {
            if (_clientViewModel.CanConnect)
            {
                await _clientViewModel.ConnectAsync();

                StartServerButton.IsEnabled = false; 
                StopServerButton.IsEnabled = false; 
                ConnectButton.IsEnabled = false;

                if (_clientViewModel.IsConnected)
                {
                    DisconnectButton.IsEnabled = true;
                }
                else
                {
                    StartServerButton.IsEnabled = true; 
                    StopServerButton.IsEnabled = false; 
                    ConnectButton.IsEnabled = true;
                }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e) 
        {
            if (_clientViewModel.CanDisconnect)
            {
                _clientViewModel.Disconnect();

                StartServerButton.IsEnabled = true; 
                StopServerButton.IsEnabled = false; 
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
            }
        }
        private void SyncUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serverViewModel.IsServerRunning)
            {
                List<string> newFiles = _serverViewModel.GetNewFiles();
                if (newFiles.Any())
                {
                    _server.BroadcastNewFiles(newFiles);
                    LogServiceViewModel.UpdateLogDetails("Sync initiated: Broadcasting new files to clients.\n");
                }
            }
            else
            {
                // Log a message if the server is not running
                LogServiceViewModel.UpdateLogDetails("Server is not running. Please start the server before syncing.\n");
            }

        }

    }
}
