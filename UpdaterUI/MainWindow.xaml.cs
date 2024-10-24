using System.Windows;
using System.Windows.Navigation;

namespace UpdaterUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var mainPage = new UpdaterUIPage();
        MainFrame.Navigate(mainPage);

        // Bind notification properties to the main window
        DataContext = mainPage.ViewModel;
    }
}
