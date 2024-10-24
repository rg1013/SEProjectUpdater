using System.ComponentModel;

namespace UpdaterViewModel;
public class Tool : INotifyPropertyChanged
{

    private string? _id;
    private string? _version;
    private string? _description;
    private string? _deprecated;
    private string? _createdBy;

    public string ID
    {
        get => _id;
        set {
            _id = value;
            OnPropertyChanged("ID");
        }
    }

    public string Version
    {
        get => _version;
        set {
            _version = value;
            OnPropertyChanged("Version");
        }
    }

    public string Description
    {
        get => _description;
        set {
            _description = value;
            OnPropertyChanged("Description");
        }
    }

    public string Deprecated
    {
        get => _deprecated;
        set {
            _deprecated = value;
            OnPropertyChanged("Deprecated");
        }
    }

    public string CreatedBy
    {
        get => _createdBy;
        set {
            _createdBy = value;
            OnPropertyChanged("CreatedBy");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
