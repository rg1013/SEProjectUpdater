/******************************************************************************
* Filename    = Server.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Server side sending and receiving files logic
*****************************************************************************/

using Networking.Communication;
using Networking;
using System.Net.Sockets;
using System.Diagnostics;

namespace Updater;

public class Server
{
    static readonly SemaphoreSlim semaphore = new(1, 1); // Allow one client at a time
    private readonly static string _serverDirectory = AppConstants.ToolsFolderPath;

    private ICommunicator? _communicator;
    public DateTime _lastSyncTime { get; set; } = DateTime.MinValue;

    public void Start(string ip, string port)
    {
        try
        {
            _communicator = CommunicationFactory.GetCommunicator(false);

            // Starting the server
            string result = _communicator.Start(ip, port);
            UpdateUILogs($"Server started on {result}");

            // Subscribing the "FileTransferHandler" for handling notifications
            _communicator.Subscribe("FileTransferHandler", new FileTransferHandler(_communicator));
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in server start: {ex.Message}");
        }
    }

    public void Stop()
    {
        try
        {
            _communicator?.Stop();
            UpdateUILogs("Server stopped.");
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error stopping the server: {ex.Message}");
        }
    }
    public void BroadcastNewFiles(List<string> ListOfNewFiles)
    {
        try
        {
            // Prepare a list to hold FileContent objects for each new file
            List<FileContent> fileContentsToSend = new List<FileContent>();

            foreach (string filename in ListOfNewFiles)
            {
                string filePath = Path.Combine(_serverDirectory, filename);

                if (!File.Exists(filePath))
                {
                    // Notify clients that the file is skipped because it doesn't exist
                    UpdateUILogs($"Skipped {filename} as it doesn't exist");
                    Trace.WriteLine($"[Updater] Warning: {filename} does not exist, skipping.");
                    continue; // Skip to the next file
                }

                string? content = Utils.ReadBinaryFile(filePath);

                if (content == null)
                {
                    UpdateUILogs($"Skipped {filename} as content of file is null");
                    Trace.WriteLine($"[Updater] Warning: Content of file {filename} is null, skipping.");
                    continue; // Skip to the next file instead of throwing an exception
                }

                Trace.WriteLine($"[Updater] Content length of {filename}: {content.Length}");

                // Serialize file content and create FileContent object
                string serializedFileContent = Utils.SerializeObject(content);
                if (string.IsNullOrEmpty(serializedFileContent))
                {
                    Trace.WriteLine($"[Updater] Warning: Serialized content for {filename} is null or empty.");
                    continue; // Skip to next file if serialization fails
                }

                // Create a FileContent object and add it to the list
                FileContent fileContent = new FileContent(filename, serializedFileContent);
                fileContentsToSend.Add(fileContent);
            }

            // Check if there are files to broadcast
            if (fileContentsToSend.Any())
            {
                // Create DataPacket for broadcasting
                DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.Broadcast, fileContentsToSend);

                // Serialize DataPacket
                string serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                // Notify clients about the broadcast
                UpdateUILogs("Broadcasting new files to all clients");
                Trace.WriteLine("[Updater] Broadcasting new files to all clients");
                //saving lastsynctime
                _lastSyncTime = DateTime.Now;

                // Send the serialized packet to all clients
                try
                {
                    _communicator?.Send(serializedDataPacket, "FileTransferHandler", null); // Broadcast to all clients
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error sending broadcast to clients: {ex.Message}");
                }
            }
            else
            {
                Trace.WriteLine("[Updater] No new files to broadcast.");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in broadcasting new files: {ex.Message}");
        }
    }

    public class FileTransferHandler : INotificationHandler
    {
        public string clientID = "";
        private readonly ICommunicator _communicator;
        private readonly Dictionary<string, TcpClient> clientConnections = new Dictionary<string, TcpClient>(); // Track clients
        private static int clientCounter = 0;
        private static Guid guid;

        public FileTransferHandler(ICommunicator communicator)
        {
            _communicator = communicator;
        }

        public static void PacketDemultiplexer(string serializedData, ICommunicator communicator, string clientID)
        {
            try
            {
                // Deserialize data
                DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

                // Check PacketType
                switch (dataPacket.DataPacketType)
                {
                    case DataPacket.PacketType.Metadata:
                        MetadataHandler(dataPacket, communicator, clientID);
                        break;
                    case DataPacket.PacketType.ClientFiles:
                        ClientFilesHandler(dataPacket, communicator, clientID);
                        break;
                    default:
                        throw new Exception("Invalid PacketType");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in PacketDemultiplexer: {ex.Message}");
            }
        }

        private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator, string clientID)
        {
            try
            {
                // Extract metadata of client directory
                List<FileContent> fileContents = dataPacket.FileContentList;

                if (fileContents.Count == 0)
                {
                    throw new Exception("No file content received in the data packet.");
                }

                // Process the first file content
                FileContent fileContent = fileContents[0];
                string? serializedContent = fileContent.SerializedContent;

                Trace.WriteLine("[Updater] " + serializedContent ?? "Serialized content is null");

                // Deserialize the client metadata
                List<FileMetadata>? metadataClient;
                if (serializedContent != null)
                {
                    metadataClient = Utils.DeserializeObject<List<FileMetadata>>(serializedContent);
                }
                else
                {
                    metadataClient = null;
                }
                if (metadataClient == null)
                {
                    throw new Exception("[Updater] Deserialized client metadata is null");
                }

                UpdateUILogs("Metadata of client received");
                Trace.WriteLine("[Updater]: Metadata from client received");

                // Generate metadata of server
                List<FileMetadata>? metadataServer = new DirectoryMetadataGenerator(_serverDirectory).GetMetadata();
                if (metadataServer == null)
                {
                    throw new Exception("Metadata server is null");
                }
                Trace.WriteLine("[Updater] Metadata from server generated");

                // Compare metadata and get differences
                DirectoryMetadataComparer comparerInstance = new DirectoryMetadataComparer(metadataServer, metadataClient);
                var differences = comparerInstance.Differences;

                // Serialize and save differences to C:\temp\ folder
                string serializedDifferences = Utils.SerializeObject(differences);
                string tempFilePath = _serverDirectory + @"\differences.xml";

                if (string.IsNullOrEmpty(serializedDifferences))
                {
                    Trace.WriteLine("[Updater] Serialization of differences failed or resulted in an empty string.");
                    return; // Exit if serialization fails
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
                    File.WriteAllText(tempFilePath, serializedDifferences);
                    UpdateUILogs($"Server: Differences file saved to {tempFilePath}");
                    Trace.WriteLine($"[" +
                        $"Updater] Differences file saved to {tempFilePath}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error saving differences file: {ex.Message}");
                }

                // Prepare data to send to client
                List<FileContent> fileContentsToSend = new List<FileContent>
                {
                    // Added difference file to be sent to client
                    new FileContent("differences.xml", serializedDifferences)
                };

                // Retrieve and add unique server files to fileContentsToSend
                foreach (string filename in comparerInstance.UniqueServerFiles)
                {
                    if(filename == fileContentsToSend[0].FileName)
                    {
                        continue;
                    }
                    string filePath = Path.Combine(_serverDirectory, filename);
                    string? content = Utils.ReadBinaryFile(filePath);

                    if (content == null)
                    {
                        Trace.WriteLine($"[Updater] Warning: Content of file {filename} is null, skipping.");
                        continue; // Skip to the next file instead of throwing an exception
                    }

                    Trace.WriteLine($"[Updater] Content length of {filename}: {content.Length}");

                    // Serialize file content and create FileContent object
                    string serializedFileContent = Utils.SerializeObject(content);
                    if (string.IsNullOrEmpty(serializedFileContent))
                    {
                        Trace.WriteLine($"[Updater] Warning: Serialized content for {filename} is null or empty.");
                        continue; // Skip to next file if serialization fails
                    }

                    FileContent fileContentToSend = new(filename, serializedFileContent);
                    fileContentsToSend.Add(fileContentToSend);
                }

                // Create DataPacket after all FileContents are ready
                DataPacket dataPacketToSend = new(DataPacket.PacketType.Differences, fileContentsToSend);
                Trace.WriteLine($"[Updater] Total files to send: {fileContentsToSend.Count}");

                // Serialize DataPacket
                string serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                try
                {
                    UpdateUILogs($"Sending {fileContentsToSend.Count} files to client and waiting to receive files from client {clientID}");
                    communicator.Send(serializedDataPacket, "FileTransferHandler", clientID); 
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                    UpdateUILogs($"Error sending data to client: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                UpdateUILogs($"Error sending data to client: {ex.Message}");
            }
        }

        private static void ClientFilesHandler(DataPacket dataPacket, ICommunicator communicator, string clientID)
        {
            guid = Guid.NewGuid();
            try
            {
                // File list
                List<FileContent> fileContentList = dataPacket.FileContentList;
                UpdateUILogs($"Received {fileContentList.Count} files from client");

                if (fileContentList.Count > 0)
                {
                    // Get files
                    foreach (FileContent fileContent in fileContentList)
                    {
                        if (fileContent != null && fileContent.SerializedContent != null && fileContent.FileName != null)
                        {
                            string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                            fileContent.FileName = guid.ToString() + fileContent.FileName;
                            string filePath = Path.Combine(_serverDirectory, fileContent.FileName);
                            bool status = Utils.WriteToFileFromBinary(filePath, content);

                            if (!status)
                            {
                                throw new Exception("Failed to write file");
                            }
                        }
                    }

                    UpdateUILogs("Successfully saved all new files");
                    Trace.WriteLine("[Updater] Successfully received client's files");

                    // Broadcast client's new files to all clients
                    dataPacket.DataPacketType = DataPacket.PacketType.Broadcast;

                    // Serialize packet
                    string serializedPacket = Utils.SerializeObject(dataPacket);

                    UpdateUILogs($"Broadcasting the new {fileContentList.Count} files");
                    Trace.WriteLine("[Updater] Broadcasting the new files");
                    try
                    {
                        communicator.Send(serializedPacket, "FileTransferHandler", null); // Broadcast to all clients
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                    }
                }
                else
                {
                    UpdateUILogs("No new files found to broadcast");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in ClientFilesHandler: {ex.Message}");
            }
        }

        public void OnDataReceived(string serializedData)
        {
            semaphore.Wait(); // Wait until it's safe to enter
            try
            {
                Trace.WriteLine("[Updater] FileTransferHandler received data");
                DataPacket deserializedData = Utils.DeserializeObject<DataPacket>(serializedData);
                if (deserializedData == null)
                {
                    Console.WriteLine("Deserialized data is null.");
                }
                else
                {
                    Trace.WriteLine("[Updater] Read received data Successfully");
                    PacketDemultiplexer(serializedData, _communicator, clientID);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Deserialization failed: {ex.Message}");
            }
            finally
            {
                semaphore.Release(); // Release the semaphore
            }
        }

        public void OnClientJoined(TcpClient socket)
        {
            try
            {
                // Generate a unique client ID
                string clientId = $"Client{Interlocked.Increment(ref clientCounter)}"; // Use Interlocked for thread safety
                clientID = clientId;
                Trace.WriteLine($"[Updater] FileTransferHandler detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
                UpdateUILogs($"Detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
                clientConnections.Add(clientId, socket); // Add client connection to the dictionary
                _communicator.AddClient(clientId, socket); // Use the unique client ID
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in OnClientJoined: {ex.Message}");
            }
        }

        public void OnClientLeft(string clientId)
        {
            try
            {
                if (clientConnections.Remove(clientId))
                {
                    UpdateUILogs("Detected client {clientId} disconnected");
                    Trace.WriteLine($"[Updater] FileTransferHandler detected client {clientId} disconnected");
                }
                else
                {
                    Trace.WriteLine($"[Updater] Client {clientId} was not found in the connections.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in OnClientLeft: {ex.Message}");
            }
        }
    }

    public static event Action<string>? OnLogUpdate;
    public static void UpdateUILogs(string message)
    {
        OnLogUpdate?.Invoke(message);
    }
}
