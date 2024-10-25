using System.ComponentModel;

namespace ViewModels;
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
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }
    }

    public string Version
    {
        get => _version;
        set {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }
    }

    public string Description
    {
        get => _description;
        set {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }
    }

    public string Deprecated
    {
        get => _deprecated;
        set {
            if (_deprecated != value)
            {
                _deprecated = value;
                OnPropertyChanged("Deprecated");
            }
        }
    }

    public string CreatedBy
    {
        get => _createdBy;
        set {
            if (_createdBy != value)
            {
                _createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
