using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Updater;

namespace ViewModels
{
    public class CloudViewModel
    {
        private LogServiceViewModel _logServiceViewModel;
        private ServerViewModel _serverViewModel;
        public CloudViewModel(LogServiceViewModel logServiceViewModel)
        {
            _logServiceViewModel = logServiceViewModel;
        }

        public void PerformCloudSync()
        {
            _logServiceViewModel.UpdateLogDetails("Cloud Starting Sync");
            var cloudData = GetDataFromCloud();//json file
            var serverData = _serverViewModel.GetServerData();
            var diffFiles = CloudHasMoreData(cloudData, serverData);
            if (diffFiles.Any())
            {
                _logServiceViewModel.UpdateLogDetails("Cloud has more Data than server. Sending JSON file to server....");
                UpdateServerWithCloudData(diffFiles);
                BroadCastNewFiles(diffFiles);
            }
        }
        public class FileData
        {
            public string FileName { get; set; }
            public long Size { get; set; }
            public DateTime LastModified { get; set; }  // Changed to DateTime
            public string FullPath { get; set; }
        }

        public static List<FileData> CloudHasMoreData(object cloudData, object serverData)
        {
            var cloudFiles = JsonSerializer.Deserialize<List<FileData>>(cloudData.ToString());
            var serverFiles = JsonSerializer.Deserialize<List<FileData>>(serverData.ToString());

            var filesOnlyInCloud = cloudFiles
                .Where(cloudFile => !serverFiles.Any(serverFile => serverFile.FileName == cloudFile.FileName))
                .ToList();

            return filesOnlyInCloud;
        }

        public void UpdateServerWithCloudData(object diffFiles)
        {
            List<FileData> files = JsonSerializer.Deserialize<List<FileData>>(diffFiles.ToString());

            foreach (var file in files)
            {
                SaveFileDataToServer(file);
            }
            _logServiceViewModel.UpdateLogDetails("Server updated with new data from cloud");
        }

        private void SaveFileDataToServer(FileData files)
        {
            // Logic to save individual file data to the server, e.g., saving to a local directory
            var destinationPath = Path.Combine(AppConstants.ToolsDirectory, files.FileName);

            // Example code to simulate file save
            File.WriteAllText(destinationPath, JsonSerializer.Serialize(files));
        }

        public void BroadCastNewFiles(List<FileData> files)
        {
            var message = new
            {
                MessageType = "NewAnalyzers",
                Data = files
            };

            var jsonMessage = JsonSerializer.Serialize(message);

            _serverViewModel.BroadCastToAllClients(jsonMessage);

            _logServiceViewModel.UpdateLogDetails("BroadCasted new cloud data to all clients");

        }
    }
}
