using System.Windows;
using ActivityTrackerViewModel;

namespace UpdaterUI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
///<summary>
/// Interaction logic for MainWindow.xaml. Acts as the View in the MVVM architecture.
/// Initializes the ViewModel and sets it as the DataContext.
///</summary>
public partial class MainWindow : Window
{
    ///<summary>
    /// Holds the ViewModel associated with this view.
    ///</summary>
    public MainViewModel ViewModel { get; }

    ///<summary>
    /// Constructor for MainWindow. Initializes components and sets up the ViewModel.
    ///</summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel(new LogService());
        DataContext = ViewModel;
    }

    ///<summary>
    /// Handles the Loaded event of the window. Updates log details upon loading.
    ///</summary>
    ///<param name="sender">The source of the event.</param>
    ///<param name="e">The RoutedEventArgs that contains the event data.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateLogDetails("Window loaded successfully.");
        ViewModel.ShowNotification("Welcome to the application!");
    }
}
