/*************************************************************************************
* Filename    = MainViewModel.cs
*
* Author      = N.Pawan Kumar
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = View Model for displaying available analyzers information on the UI
**************************************************************************************/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ActivityTrackerViewModel;

///<summary>
/// The ViewModel that serves as the data context for the MainWindow. It provides properties
/// and commands that the view binds to and communicates changes via the INotifyPropertyChanged interface.
///</summary>
public class MainViewModel : INotifyPropertyChanged
{
    private string _logDetails = "";
    private string _notificationMessage = "";
    private bool _notificationVisible = false;
    private DispatcherTimer _timer; // Timer to auto-hide notifications

    ///<summary>
    /// Initializes a new instance of the MainViewModel class.
    ///</summary>
    public MainViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) }; // Notification shows for 5 seconds
        _timer.Tick += (sender, e) => { HideNotification(); };
    }

    ///<summary>
    /// Gets or sets the log details, which are displayed in the UI and updated upon changes.
    ///</summary>
    public string LogDetails
    {
        get => _logDetails;
        set {
            _logDetails = value;
            OnPropertyChanged(nameof(LogDetails));
        }
    }

    ///<summary>
    /// Gets or sets the message for the notification popup.
    ///</summary>
    public string NotificationMessage
    {
        get => _notificationMessage;
        set {
            _notificationMessage = value;
            OnPropertyChanged(nameof(NotificationMessage));
        }
    }

    ///<summary>
    /// Gets or sets the visibility of the notification popup.
    ///</summary>
    public bool NotificationVisible
    {
        get => _notificationVisible;
        set {
            _notificationVisible = value;
            OnPropertyChanged(nameof(NotificationVisible));
        }
    }

    ///<summary>
    /// Appends a message to the log details.
    ///</summary>
    ///<param name="message">The message to append to the log.</param>
    public void UpdateLogDetails(string message)
    {
        LogDetails += "\n" + message;
    }

    ///<summary>
    /// Displays a notification with a specified message.
    ///</summary>
    ///<param name="message">The message to display in the notification.</param>
    public void ShowNotification(string message)
    {
        NotificationMessage = message;
        NotificationVisible = true;
        _timer.Start();
    }

    ///<summary>
    /// Hides the notification popup and stops the auto-hide timer.
    ///</summary>
    private void HideNotification()
    {
        NotificationVisible = false;
        _timer.Stop();
    }

    ///<summary>
    /// Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler? PropertyChanged; // Nullable event handler

    ///<summary>
    /// Notifies listeners about property changes.
    ///</summary>
    ///<param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty)); 
    }
}
