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
/// Class to populate list of available tools for server-side operations
/// </summary>
public class ToolListViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Tool>? AvailableToolsList { get; set; }

    /// <summary>
    /// Loads available tools from the specified folder using the DllLoader.
    /// Populates the AvailableToolsList property with the retrieved data.
    /// </summary>
    public ToolListViewModel() => LoadAvailableTools();
    public void LoadAvailableTools()
    {
        Dictionary<string, List<string>> ToolsInfoMap = ToolAssemblyLoader.LoadToolsFromFolder(AppConstants.ToolsFolderPath);

        if (ToolsInfoMap.Count > 0)
        {
            int rowCount = ToolsInfoMap.Values.First().Count;
            AvailableToolsList = [];

            for (int i = 0; i < rowCount; i++)
            {
                var newTool = new Tool {
                    ID = ToolsInfoMap["Id"][i],
                    Version = ToolsInfoMap["Version"][i],
                    Description = ToolsInfoMap["Description"][i],
                    Deprecated = ToolsInfoMap["IsDeprecated"][i],
                    CreatedBy = ToolsInfoMap["CreatorName"][i]
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
