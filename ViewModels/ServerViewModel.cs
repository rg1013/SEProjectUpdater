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
using System.Xml;
using Updater;
using System.Text.Json;
namespace ViewModels;
public class ServerViewModel : INotifyPropertyChanged
{
    private Server _server;
    private LogServiceViewModel _logServiceViewModel;
    private CloudViewModel _cloudViewModel;
    private Mutex _mutex;

    public ServerViewModel(LogServiceViewModel logServiceViewModel)
    {
        _server = new Server();
        _logServiceViewModel = logServiceViewModel;
        Server.NotificationReceived += AddLogMessage;

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
            _cloudViewModel.PerformCloudSync(); 
        }
        else
        {
            _logServiceViewModel.UpdateLogDetails("Server is already running on another instance.");
        }
    }

    public void StopServer()
    {
        _server.Stop();
        _mutex.ReleaseMutex();
    }

    private void AddLogMessage(string message)
    {
        _logServiceViewModel.UpdateLogDetails(message);
    }

    public object GetServerData()
    {
        string serverFolderPath = AppConstants.ToolsDirectory; // Adjust if your temp folder is located differently
        var fileDataList = new List<object>();

        if (Directory.Exists(serverFolderPath))
        {
            foreach (string filePath in Directory.GetFiles(serverFolderPath))
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileData = new
                    {
                        FileName = fileInfo.Name,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        FullPath = fileInfo.FullName
                    };

                    fileDataList.Add(fileData);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error accessing file {filePath}: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine("Server directory not found.");
        }

        // Serialize the list of file data to JSON
        string jsonResult = JsonSerializer.Serialize(fileDataList, new JsonSerializerOptions { WriteIndented = true });
        return jsonResult;
    }

    public void BroadCastToAllClients(object file)
    {

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
