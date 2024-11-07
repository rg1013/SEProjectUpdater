/******************************************************************************
* Filename    = ClientViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Client side logic
*****************************************************************************/

using Networking;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Updater;

namespace ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private Client _client;
        private bool _isConnected;
        private LogServiceViewModel _logServiceViewModel;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanConnect));
                    OnPropertyChanged(nameof(CanDisconnect));
                }
            }
        }

        public bool CanConnect => !IsConnected;
        public bool CanDisconnect => IsConnected;

        public ClientViewModel(LogServiceViewModel logServiceViewModel)
        {
            _logServiceViewModel = logServiceViewModel;
            _client = new Client(CommunicationFactory.GetCommunicator(true));
            Client.OnLogUpdate += UpdateLog; // Subscribe to log updates
        }

        private void UpdateLog(string logMessage)
        {
            _logServiceViewModel.UpdateLogDetails(logMessage); // Update log through LogServiceViewModel
        }

        public async Task ConnectAsync()
        {
            string result = await _client.StartAsync("10.32.2.232", "60091");
            if (result == "success")
            {
                IsConnected = true;
                _client.Subscribe();
            }
            else
            {
                UpdateLog("Failed to connect to server."); // Log the failure
            }
        }

        public void Disconnect()
        {
            _client.Stop();
            IsConnected = false;
            UpdateLog("Disconnected from server."); // Log the disconnection
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
