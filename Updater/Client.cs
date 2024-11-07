/******************************************************************************
* Filename    = Client.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Client side sending and receiving files logic
*****************************************************************************/

using Networking.Communication;
using Networking;
using System.Diagnostics;

namespace Updater;

public class Client
{
    private readonly ICommunicator _communicator;
    private readonly static string _clientDirectory = @"C:\received";

    public Client(ICommunicator communicator)
    {
        _communicator = communicator;
    }

    public async Task<string> StartAsync(string ipAddress, string port)
    {
        UpdateUILogs($"Attempting to connect to server at {ipAddress}:{port}...");
        string result = await Task.Run(() => _communicator.Start(ipAddress, port));
        if (result == "success")
        {
            UpdateUILogs("Successfully connected to server.");
        }
        else
        {
            UpdateUILogs("Failed to connect to server.");
        }
        return result;
    }

    public void Subscribe()
    {
        _communicator.Subscribe("FileTransferHandler", new FileTransferHandler(_communicator));
        SyncUp();
    }

    public void SyncUp()
    {
        string serializedMetaData = Utils.SerializedMetadataPacket();

        // Sending data as FileTransferHandler
        UpdateUILogs("Syncing Up with the server!");
        Trace.WriteLine("[Updater] Sending metadata of client as FileTransferHandler...");
        _communicator.Send(serializedMetaData, "FileTransferHandler", null);
    }

    public void Stop()
    {
        UpdateUILogs("Client disconnected");
        _communicator.Stop();
    }


    public class FileTransferHandler : INotificationHandler
    {
        private readonly ICommunicator _communicator;
        private static Guid guid;

        public FileTransferHandler(ICommunicator communicator)
        {
            _communicator = communicator;
        }

        public static void PacketDemultiplexer(string serializedData, ICommunicator communicator)
        {
            try
            {
                DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

                // Check PacketType
                switch (dataPacket.DataPacketType)
                {
                    case DataPacket.PacketType.Broadcast:
                        BroadcastHandler(dataPacket, communicator);
                        break;
                    case DataPacket.PacketType.Differences:
                        DifferencesHandler(dataPacket, communicator);
                        break;
                    default:
                        throw new Exception("Invalid PacketType");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in PacketDemultiplexer: {ex.Message}");
            }
        }

        private static void BroadcastHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            try
            {
                // File list
                List<FileContent> fileContentList = dataPacket.FileContentList;

                UpdateUILogs($"Received {fileContentList.Count} new files from server");

                // Get files
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent != null && fileContent.SerializedContent != null && fileContent.FileName != null)
                    {
                        // Deserialize the content based on expected format
                        string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                        string filePath = Path.Combine(_clientDirectory, fileContent.FileName);
                        UpdateUILogs($"Saving files to {filePath}");
                        bool status = Utils.WriteToFileFromBinary(filePath, content);
                        if (!status)
                        {
                            throw new Exception("Failed to write file");
                        }
                    }
                }
                UpdateUILogs("You are up-to-date with the server!");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in BroadcastHandler: {ex.Message}");
            }
        }

        private static void ClientFilesHandler(DataPacket dataPacket)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in ClientFilesHandler: {ex.Message}");
            }
        }

        private static void DifferencesHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            guid = Guid.NewGuid();
            try
            {
                List<FileContent> fileContentList = dataPacket.FileContentList;
                UpdateUILogs($"Recieved {fileContentList.Count - 1} files from Server");

                // Deserialize the 'differences' file content
                FileContent differenceFile = fileContentList[0];
                string? serializedDifferences = differenceFile.SerializedContent;
                string? differenceFileName = differenceFile.FileName;

                if (serializedDifferences == null)
                {
                    throw new Exception("[Updater] SerializedContent is null");
                }

                // Deserialize to List<MetadataDifference>
                List<MetadataDifference> differencesList = Utils.DeserializeObject<List<MetadataDifference>>(serializedDifferences);

                // Process additional files in the list
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent == differenceFile)
                    {
                        continue;

                    }
                    if (fileContent != null && fileContent.SerializedContent != null)
                    {
                        string content;
                        // Check if the SerializedContent is base64 or XML by detecting XML declaration
                        if (fileContent.SerializedContent.StartsWith("<?xml"))
                        {
                            // Directly deserialize XML content
                            content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                        }
                        else
                        {
                            // Decode base64 content
                            string decodedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(fileContent.SerializedContent));
                            content = Utils.DeserializeObject<string>(decodedContent);
                        }

                        string filePath = Path.Combine(_clientDirectory, guid.ToString() + fileContent.FileName); //+ Guid.NewGuid().ToString()
                        bool status = Utils.WriteToFileFromBinary(filePath, content);
                        if (!status)
                        {
                            throw new Exception("[Updater] Failed to write file");
                        }
                        else
                        {
                            UpdateUILogs($"Writing file {filePath}");
                        }
                    }
                }

                // Using the deserialized differences list to retrieve UniqueClientFiles
                List<string> filenameList = differencesList
                    .Where(difference => difference != null && difference.Key == "-1")
                    .SelectMany(difference => difference.Value?.Select(fileDetail => fileDetail.FileName) ?? new List<string>())
                    .Distinct()
                    .ToList();

                UpdateUILogs("Processing requested files from Server");

                // Create list of FileContent to send back
                List<FileContent> fileContentToSend = [];

                foreach (string filename in filenameList)
                {
                    if (filename == differenceFileName)
                    {
                        continue;
                    }
                    if (filename != null)
                    {
                        string filePath = Path.Combine(_clientDirectory, filename);
                        string? content = Utils.ReadBinaryFile(filePath);

                        if (content == null)
                        {
                            throw new Exception("Failed to read file");
                        }

                        string? serializedContent = Utils.SerializeObject(content);
                        if (serializedContent == null)
                        {
                            throw new Exception("Failed to serialize content");
                        }

                        FileContent fileContent = new FileContent(filename, serializedContent);
                        fileContentToSend.Add(fileContent);
                    }
                }

                // Create DataPacket to send
                DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.ClientFiles, fileContentToSend);

                // Serialize and send DataPacket
                string? serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                UpdateUILogs($"Sending {fileContentToSend.Count} requested files to server");
                Trace.WriteLine("Sending files to server");
                communicator.Send(serializedDataPacket, "FileTransferHandler", null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in DifferencesHandler: {ex.Message}");
            }
        }

        public void OnDataReceived(string serializedData)
        {
            try
            {
                
                Trace.WriteLine($"[Updater] FileTransferHandler received data");
                UpdateUILogs($"FileTransferHandler received data");
                PacketDemultiplexer(serializedData, _communicator);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in OnDataReceived: {ex.Message}");
            }
        }
    }

    public static event Action<string>? OnLogUpdate;

    public static void UpdateUILogs(string data)
    {
        OnLogUpdate?.Invoke(data); 
    }
}
