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

using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using Updater;
using ViewModels;

namespace UI.Views
{
    /// <summary> 
    /// Interaction logic for UpdaterPage.xaml 
    /// </summary> 
    public partial class UpdaterPage : Page
    {
        public LogServiceViewModel _logServiceViewModel { get; }
        private FileChangeNotifier _analyzerNotificationService;
        private ToolListViewModel _toolListViewModel;
        private CloudViewModel _cloudViewModel;
        private ServerViewModel _serverViewModel; // Added server view model 
        private ClientViewModel _clientViewModel; // Added client view model 

        public UpdaterPage()
        {
            InitializeComponent();
            _toolListViewModel = new ToolListViewModel();
            _toolListViewModel.LoadAvailableTools();
            ListView listView = (ListView)this.FindName("ToolViewList");
            listView.DataContext = _toolListViewModel;

            _analyzerNotificationService = new FileChangeNotifier();
            _analyzerNotificationService.MessageReceived += OnMessageReceived;

            _logServiceViewModel = new LogServiceViewModel();
            DataContext = _logServiceViewModel;

            _serverViewModel = new ServerViewModel(_logServiceViewModel); // Initialize the server view model 
            _clientViewModel = new ClientViewModel(_logServiceViewModel); // Initialize the client view model 
        }

        private void OnMessageReceived(string message)
        {
            _toolListViewModel.LoadAvailableTools(); // Refresh the tool list on message receipt 
            _logServiceViewModel.ShowNotification(message); // Show received message as a notification 
            _logServiceViewModel.UpdateLogDetails(message); // Update log with received message 
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _toolListViewModel.LoadAvailableTools(); // Load tools when the page loads 
            _logServiceViewModel.UpdateLogDetails("Page loaded successfully.\n");
            _logServiceViewModel.ShowNotification("Welcome to the application!"); // Welcome notification 
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the server can be started 
            if (_serverViewModel.CanStartServer())
            {
                string ip = AppConstants.ServerIP; // Assume you have an IpTextBox for IP input 
                string port = AppConstants.Port; // Assume you have a PortTextBox for Port input 

                _serverViewModel.StartServer(ip, port); // Call to start the server 

                StartServerButton.IsEnabled = false; // Disable Start button 
                StopServerButton.IsEnabled = true; // Enable Stop button 

                // Disable all other buttons
                DisconnectButton.IsEnabled = false; // Disable Disconnect button
                ConnectButton.IsEnabled = false; // Disable Connect button
                                                 // Add any additional buttons you want to disable here
            }
            else
            {
                _logServiceViewModel.UpdateLogDetails("Server is already running on another instance.\n");
            }
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e) // Handler for stop button click 
        {
            _serverViewModel.StopServer(); // Call to stop the server 

            // Enable the Start Server button after stopping the server 
            StartServerButton.IsEnabled = true;
            StopServerButton.IsEnabled = false; // Disable Stop button 
                                                // Disable all other buttons
            DisconnectButton.IsEnabled = true; // Disable Disconnect button
            ConnectButton.IsEnabled = true; // Disable Connect button

        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e) // Handler for connect button click 
        {
            if (_clientViewModel.CanConnect)
            {
                _logServiceViewModel.UpdateLogDetails("Connecting to server...\n"); // Log connecting message
                await _clientViewModel.ConnectAsync();

                // Disable all other buttons except for Disconnect
                StartServerButton.IsEnabled = false; // Disable Start button
                StopServerButton.IsEnabled = false; // Disable Stop button
                ConnectButton.IsEnabled = false; // Disable Connect button

                // Enable Disconnect button
                if (_clientViewModel.IsConnected)
                {
                    _logServiceViewModel.UpdateLogDetails("Successfully connected to server!\n"); // Log successful connection
                    DisconnectButton.IsEnabled = true; // Enable Disconnect button
                }
                else
                {
                    _logServiceViewModel.UpdateLogDetails("Failed to connect to server.\n"); // Log failure
                    StartServerButton.IsEnabled = true; // Disable Start button
                    StopServerButton.IsEnabled = false; // Disable Stop button
                    ConnectButton.IsEnabled = true;
                }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e) // Handler for disconnect button click 
        {
            if (_clientViewModel.CanDisconnect)
            {
                _clientViewModel.Disconnect();
                _logServiceViewModel.UpdateLogDetails("Disconnected from server.\n"); // Log disconnection message

                // Enable buttons after disconnection
                StartServerButton.IsEnabled = true; // Enable Start button
                StopServerButton.IsEnabled = false; // Disable Stop button
                ConnectButton.IsEnabled = true; // Enable Connect button
                DisconnectButton.IsEnabled = false; // Disable Disconnect button
            }
        }

        private async void SyncCloudButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the Sync button to prevent multiple syncs at the same time
            CloudSyncButton.IsEnabled = false;

            string ip = AppConstants.ServerIP;
            string port = AppConstants.Port;

            if (_serverViewModel.CanStartServer())
            {
                _logServiceViewModel.UpdateLogDetails("Cloud sync starting...");

                try
                {
                    // Start the server
                    _serverViewModel.StartServer(ip, port);
                    _logServiceViewModel.UpdateLogDetails("Server started for cloud sync.");

                    // Perform cloud sync asynchronously
                    await Task.Run(() => _cloudViewModel.PerformCloudSync());

                    _logServiceViewModel.UpdateLogDetails("Cloud sync completed.");
                }
                catch (Exception ex)
                {
                    _logServiceViewModel.UpdateLogDetails($"Error during cloud sync: {ex.Message}");
                }
                finally
                {
                    // Ensure the server is stopped and button is re-enabled
                    _serverViewModel.StopServer();
                    _logServiceViewModel.UpdateLogDetails("Server stopped after cloud sync.");

                    CloudSyncButton.IsEnabled = true; // Re-enable Sync button
                }
            }
            else
            {
                _logServiceViewModel.UpdateLogDetails("Cannot start cloud sync as the server is already running.");
                CloudSyncButton.IsEnabled = true; // Re-enable Sync button if sync cannot start
            }
        }






    }
}
