/******************************************************************************
* Filename    = Tool.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for tool
*****************************************************************************/

using System.ComponentModel;

namespace ViewModels;

/// <summary>
/// ViewModel representing a Tool with various properties.
/// </summary>
public class Tool : INotifyPropertyChanged
{
    private string? _id;
    private string? _version;
    private string? _description;
    private string? _deprecated;
    private string? _createdBy;

    /// <summary>
    /// Gets or sets the unique identifier for the tool.
    /// </summary>
    public string ID
    {
        get => _id ?? "";
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the tool as a string representation.
    /// </summary>
    public string Version
    {
        get => _version ?? "";
        set
        {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
    }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    public string Description
    {
        get => _description ?? "";
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the tool is deprecated. Expected values are "true" or "false".
    /// </summary>
    public string Deprecated
    {
        get => _deprecated ?? "";
        set
        {
            if (_deprecated != value)
            {
                _deprecated = value;
                OnPropertyChanged(nameof(Deprecated));
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the creator of the tool.
    /// </summary>
    public string CreatedBy
    {
        get => _createdBy ?? "";
        set
        {
            if (_createdBy != value)
            {
                _createdBy = value;
                OnPropertyChanged(nameof(CreatedBy));
            }
        }
    }

    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
