using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace UpdaterViewModel;

public class FileChangeNotifier : INotifyPropertyChanged
{
    private string _messageStatus;
    private FileSystemWatcher _fileWatcher;
    private List<string> _createdFiles;
    private List<string> _deletedFiles;
    private List<string> _movedFiles;
    private Timer _timer;

    public event Action<string> MessageReceived;

    public FileChangeNotifier()
    {
        _createdFiles = new List<string>();
        _deletedFiles = new List<string>();
        // _movedFiles = new List<string>();
        StartMonitoring();
    }

    public string MessageStatus
    {
        get => _messageStatus;
        set
        {
            _messageStatus = value;
            OnPropertyChanged(nameof(MessageStatus));
        }
    }

    private void StartMonitoring()
    {
        //Path to folder to monitor
        string folderPath = @"C:\temp";
        _fileWatcher = new FileSystemWatcher
        {
            Path = folderPath,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*.*"
        };

        _fileWatcher.Created += OnFileCreated;
        _fileWatcher.Deleted += OnFileDeleted;
        //_fileWatcher.Renamed += OnFileMoved;
        _fileWatcher.EnableRaisingEvents = true;

        MessageStatus = $"Monitoring folder: {folderPath}";

        //Initialize timer with 1 second interval (adjust as necessary)
        _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Add the file to the list
        lock (_createdFiles)
        {
            _createdFiles.Add(e.FullPath);
        }

        // Restart the timer for debouncing
        _timer.Change(1000, Timeout.Infinite); // 1 second delay before processing (adjust as needed)
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        // Add the file to the list
        lock (_deletedFiles)
        {
            _deletedFiles.Add(e.FullPath);
        }

        // Restart the timer for debouncing
        _timer.Change(1000, Timeout.Infinite); // 1 second delay before processing (adjust as needed)
    }
    /*************
    private void OnFileMoved(object sender, RenamedEventArgs e)
    {
        // Add the file to the list
        lock (_movedFiles)
        {
            _movedFiles.Add($"{e.OldFullPath} -> {e.FullPath}");
        }

        // Restart the timer for debouncing
        _timer.Change(1000, Timeout.Infinite); // 1 second delay before processing (adjust as needed)
    }
    **********/

    private void OnTimerElapsed(object state)
    {
        List<string> filesToProcess;

        // Lock and extract the current batch of files
        lock (_createdFiles)
        {
            filesToProcess = new List<string>(_createdFiles);
            _createdFiles.Clear();
        }

        List<string> deletedFilesToProcess;

        lock (_deletedFiles)
        {
            deletedFilesToProcess = new List<string>(_deletedFiles);
            _deletedFiles.Clear();
        }


        StringBuilder message = new StringBuilder();

        if (filesToProcess.Any())
        {
            string fileList = string.Join(", ", filesToProcess.Select(Path.GetFileName));
            message.AppendLine($"Files created: {fileList}");
        }

        if (deletedFilesToProcess.Any())
        {
            string deletedFileList = string.Join(", ", deletedFilesToProcess.Select(Path.GetFileName));
            message.AppendLine($"Files removed: {deletedFileList}");
        }
        if (message.Length > 0)
        {
            string v = message.ToString();
            MessageStatus = v;
            MessageReceived?.Invoke(v);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
