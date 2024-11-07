/******************************************************************************
* Filename    = ServerViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Server side logic
*****************************************************************************/

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Updater;

namespace ViewModels;
public class ServerViewModel : INotifyPropertyChanged
{
    private Server _server;
    private LogServiceViewModel _logServiceViewModel;
    private Mutex _mutex;

    public ServerViewModel(LogServiceViewModel logServiceViewModel)
    {
        _server = new Server();
        _logServiceViewModel = logServiceViewModel;
        Server.OnLogUpdate += AddLogMessage;

        // Create a named mutex
        _mutex = new Mutex(false, "Global\\MyUniqueServerMutexName");
    }

    public bool CanStartServer()
    {
        return _mutex.WaitOne(0); // Check if the mutex can be acquired
    }

    public void StartServer(string ip, string port)
    {
        if (CanStartServer())
        {
            Task.Run(() => _server.Start(ip, port));
            IsServerRunning = true;
        }
        else
        {
            _logServiceViewModel.UpdateLogDetails("Server is already running on another instance.");
        }
    }

    public void StopServer()
    {
        _server.Stop();
        IsServerRunning = false;
        _mutex.ReleaseMutex();
    }

    private void AddLogMessage(string message)
    {
        _logServiceViewModel.UpdateLogDetails(message);
    }

    /***************************************************************************/
    //new classes
    public List<string> GetNewFiles()
    {
        List<string> newFiles = new List<string>();

        // Get all files in the server directory
        var filesInDirectory = Directory.GetFiles(@"C:\temp");

        foreach (var file in filesInDirectory)
        {
            // Check the last modified time
            DateTime lastModified = File.GetLastWriteTime(file);

            // Compare with the last sync time (you could store this timestamp for each sync)
            if (lastModified > _server._lastSyncTime)  // _lastSyncTime would be tracked and updated after each sync
            {
                newFiles.Add(Path.GetFileName(file)); // Add file to the new files list
            }
        }
        return newFiles;
    }

    private bool _isServerRunning;
    /// <summary>
    /// For using sync button need to check first whether server is running or not.
    /// </summary>
    public bool IsServerRunning
    {
        get => _isServerRunning;
        private set
        {
            if (_isServerRunning != value)
            {
                _isServerRunning = value;
                // Notify UI about property change if using INotifyPropertyChanged
                OnPropertyChanged(nameof(IsServerRunning));
            }
        }
    }
    /********************************************************************/
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
