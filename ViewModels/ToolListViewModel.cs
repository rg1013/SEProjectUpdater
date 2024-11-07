/*************************************************************************************
* Filename    = ToolListViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = View Model for displaying available analyzers information on the UI
**************************************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Updater;

namespace ViewModels;

/// <summary>
/// Class to populate list of available analyzers for server-side operations
/// </summary>
public class ToolListViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Tool>? AvailableToolsList { get; set; }

    /// <summary>
    /// Loads available analyzers from the specified folder using the DllLoader.
    /// Populates the AnalyzerInfo property with the retrieved data.
    /// </summary>
    public ToolListViewModel() => LoadAvailableTools();
    public void LoadAvailableTools()
    {
        Dictionary<string, List<string>> hashMap = ToolAssemblyLoader.LoadToolsFromFolder(@"C:\temp");

        if (hashMap.Count > 0)
        {
            int rowCount = hashMap.Values.First().Count;
            AvailableToolsList = [];

            for (int i = 0; i < rowCount; i++)
            {
                var newTool = new Tool {
                    ID = hashMap["Id"][i],
                    Version = hashMap["Version"][i],
                    Description = hashMap["Description"][i],
                    Deprecated = hashMap["IsDeprecated"][i],
                    CreatedBy = hashMap["CreatorName"][i]
                };

                // Check if the tool is already in the list
                bool isDuplicate = AvailableToolsList.Any(tool =>
                    tool.ID == newTool.ID && tool.Version == newTool.Version);

                // Add the tool only if it's not a duplicate
                if (!isDuplicate)
                {
                    AvailableToolsList.Add(newTool);
                }
            }
            Trace.WriteLine("Available Tools information updated successfully");
        }
        else
        {
            Trace.WriteLine("No files found");
        }

        OnPropertyChanged(nameof(AvailableToolsList));
    }

    /// <summary>
    /// Gets or sets the list of available analyzers.
    /// </summary>
    public ObservableCollection<Tool> ToolInfo
    {
        get => AvailableToolsList ?? [];
        set {
            AvailableToolsList = value;
            OnPropertyChanged(nameof(ToolInfo));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
