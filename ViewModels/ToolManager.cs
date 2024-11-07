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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ViewModels
{
    /// <summary>
    /// Manages the collection of tools and synchronizes any changes to the cloud.
    /// </summary>
    public class ToolManager
    {
        /// <summary>
        /// Observable collection of available tools that can notify when items are added, removed, or changed.
        /// </summary>
        public ObservableCollection<Tool> AvailableToolsList { get; set; } = new ObservableCollection<Tool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolManager"/> class and sets up event handling for collection changes.
        /// </summary>
        public ToolManager()
        {
            // Subscribes to changes in the AvailableToolsList collection to monitor adds and removes
            AvailableToolsList.CollectionChanged += OnToolsCollectionChanged;
        }

        /// <summary>
        /// Handles changes in the <see cref="AvailableToolsList"/> collection.
        /// When a new tool is added, it sends it to the cloud.
        /// When a tool is removed, it unsubscribes from its PropertyChanged event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing information about the collection change.</param>
        private async void OnToolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Handles newly added items
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (Tool newTool in e.NewItems)
                {
                    // Subscribes to PropertyChanged events on each new tool for monitoring updates
                    newTool.PropertyChanged += Tool_PropertyChanged;
                    await SendToolToCloud(newTool);
                }
            }

            // Handles removed items
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (Tool oldTool in e.OldItems)
                {
                    // Unsubscribes from PropertyChanged events on each removed tool to stop monitoring updates
                    oldTool.PropertyChanged -= Tool_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Event handler for changes in the properties of individual tools.
        /// When a property changes, the tool is sent to the cloud to update its current state.
        /// </summary>
        /// <param name="sender">The tool that has been modified.</param>
        /// <param name="e">The event data containing the name of the changed property.</param>
        private async void Tool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Tool updatedTool)
            {
                // Sends the updated tool to the cloud to synchronize the change
                await SendToolToCloud(updatedTool);
            }
        }
        /*****Optional function******/
        /// <summary>
        /// Sends the entire collection of tools to the cloud as a batch. 
        /// Call this method during specific events (like initial loading or periodically) 
        /// to ensure the entire collection is synchronized with the cloud.
        /// Note: This method requires a cloud endpoint capable of handling a list of tools.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendAllToolsToCloudAsync()
        {
            try
            {
                // Serialize the entire AvailableToolsList as JSON for sending
                string jsonData = JsonSerializer.Serialize(AvailableToolsList);
                // Replace with your actual endpoint
                var url = "https://your-cloud-endpoint.com/api/tools/batch"; 

                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_ACCESS_TOKEN");

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("All tool data successfully sent to the cloud.");
                }
                else
                {
                    Console.WriteLine("Failed to send data: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending all tool data to the cloud: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends an individual tool to the cloud.
        /// This method is called when a tool is added or updated in the collection.
        /// </summary>
        /// <param name="tool">The <see cref="Tool"/> instance to be sent to the cloud.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task SendToolToCloud(Tool tool)
        {
            try
            {
                // Serialize the individual tool as JSON for sending
                string jsonData = JsonSerializer.Serialize(tool);
                // Replace with your actual endpoint
                //cloud team needs to update.
                var url = "https://your-cloud-endpoint.com/api/tools";

                using HttpClient client = new HttpClient();
                // Optionally add authorization headers here if needed
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_ACCESS_TOKEN");

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Tool data successfully sent to the cloud.");
                }
                else
                {
                    Console.WriteLine("Failed to send data: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending tool data to the cloud: " + ex.Message);
            }
        }
    }
}
