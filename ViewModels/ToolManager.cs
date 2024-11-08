/******************************************************************************
* Filename    = ToolManager.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Updates the cloud if any change in AvailableToolsList.
*****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ViewModels
{
    /// <summary>
    /// Manages the collection of tools and synchronizes any changes to the cloud.
    /// </summary>
    public class ToolManager
    {
        private readonly UpdaterCloudProxy _cloudProxy;

        /// <summary>
        /// Observable collection of available tools that can notify when items are added, removed, or changed.
        /// </summary>
        public ObservableCollection<Tool> AvailableToolsList { get; set; } = new ObservableCollection<Tool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolManager"/> class and sets up event handling for collection changes.
        /// </summary>
        public ToolManager()
        {
            _cloudProxy = new UpdaterCloudProxy();
            AvailableToolsList.CollectionChanged += OnToolsCollectionChanged;
        }

        /// <summary>
        /// Handles changes in the <see cref="AvailableToolsList"/> collection.
        /// When a new tool is added, it sends it to the cloud.
        /// When a tool is removed, it unsubscribes from its PropertyChanged event.
        /// </summary>
        private async void OnToolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (Tool newTool in e.NewItems)
                {
                    newTool.PropertyChanged += Tool_PropertyChanged;
                    await SendToolToCloud(newTool);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (Tool oldTool in e.OldItems)
                {
                    oldTool.PropertyChanged -= Tool_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Event handler for changes in the properties of individual tools.
        /// When a property changes, the tool is sent to the cloud to update its current state.
        /// </summary>
        private async void Tool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Tool updatedTool)
            {
                await SendToolToCloud(updatedTool);
            }
        }

        /// <summary>
        /// Sends the entire collection of tools to the cloud.
        /// </summary>
        public async Task SendAllToolsToCloudAsync()
        {
            foreach (var tool in AvailableToolsList)
            {
                await SendToolToCloud(tool);
            }
        }

        /// <summary>
        /// Sends an individual tool to the cloud.
        /// </summary>
        private async Task SendToolToCloud(Tool tool)
        {
            try
            {
                //  `tool` has a method or property that returns its identifier or URI.
                string toolUri = $"tools/{tool.ID}"; 
                var responseDict = await _cloudProxy.Post(toolUri, tool);

                if (responseDict.ContainsKey("dataUri"))
                {
                    Console.WriteLine($"Tool data successfully sent to the cloud at URI: {responseDict["dataUri"]}");
                }
                else
                {
                    Console.WriteLine("Unexpected response: 'dataUri' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tool data to the cloud: {ex.Message}");
            }
        }
    }
}

