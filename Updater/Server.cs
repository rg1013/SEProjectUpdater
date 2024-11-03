using Networking.Communication;
using Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Updater;

public class Server
{
    static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Allow one client at a time
    static int clientCounter = 0; // Counter for unique client IDs

    private ICommunicator _communicator;

    public static event Action<string>? NotificationReceived; // Event to notify the view model

    public void Start(string ip, string port)
    {
        try
        {
            _communicator = CommunicationFactory.GetCommunicator(false);

            // Starting the server
            string result = _communicator.Start(ip, port);
            NotifyClients($"Server started on {result}");

            // Subscribing the "ClientMetadataHandler" for handling notifications
            _communicator.Subscribe("ClientMetadataHandler", new ClientMetadataHandler(_communicator));
        }
        catch (Exception ex)
        {
            NotifyClients($"Error in server start: {ex.Message}");
        }
    }

    public void Stop()
    {
        try
        {
            _communicator.Stop();
            NotifyClients("Server stopped.");
        }
        catch (Exception ex)
        {
            NotifyClients($"Error stopping the server: {ex.Message}");
        }
    }

    public class ClientMetadataHandler : INotificationHandler
    {
        public string clientID = "";
        private readonly ICommunicator _communicator;
        private readonly Dictionary<string, TcpClient> clientConnections = new Dictionary<string, TcpClient>(); // Track clients
        private static int clientCounter = 0;

        public ClientMetadataHandler(ICommunicator communicator)
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
                    case DataPacket.PacketType.Broadcast:
                        BroadcastHandler(dataPacket);
                        break;
                    case DataPacket.PacketType.ClientFiles:
                        ClientFilesHandler(dataPacket, communicator, clientID);
                        break;
                    case DataPacket.PacketType.Differences:
                        DifferencesHandler(dataPacket);
                        break;
                    default:
                        throw new Exception("Invalid PacketType");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PacketDemultiplexer: {ex.Message}");
            }
        }

        private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator, string clientID)
        {
            try
            {
                // Extract metadata of client directory
                List<FileContent> fileContents = dataPacket.FileContentList;

                if (!fileContents.Any())
                {
                    throw new Exception("No file content received in the data packet.");
                }

                // Process the first file content
                FileContent fileContent = fileContents[0];
                string? serializedContent = fileContent.SerializedContent;

                Trace.WriteLine("[Updater] " + serializedContent ?? "Serialized content is null");

                // Deserialize the client metadata
                List<FileMetadata>? metadataClient = Utils.DeserializeObject<List<FileMetadata>>(serializedContent);
                if (metadataClient == null)
                {
                    throw new Exception("Deserialized client metadata is null");
                }
                Trace.WriteLine("[Updater]: Metadata from client received");

                // Generate metadata of server
                List<FileMetadata>? metadataServer = new DirectoryMetadataGenerator(@"C:\temp").GetMetadata();
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
                string tempFilePath = @"C:\temp\differences.xml";

                if (string.IsNullOrEmpty(serializedDifferences))
                {
                    Trace.WriteLine("[Updater] Serialization of differences failed or resulted in an empty string.");
                    return; // Exit if serialization fails
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
                    File.WriteAllText(tempFilePath, serializedDifferences);
                    NotifyClients($"Differences file saved to {tempFilePath}");
                    Trace.WriteLine($"[Updater] Differences file saved to {tempFilePath}");
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
                    string filePath = Path.Combine(@"C:\temp", filename);
                    string? content = Utils.ReadBinaryFile(filePath);

                    if (content == null)
                    {
                        Console.WriteLine($"Warning: Content of file {filename} is null, skipping.");
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

                    FileContent fileContentToSend = new FileContent(filename, serializedFileContent);
                    fileContentsToSend.Add(fileContentToSend);
                }

                // Create DataPacket after all FileContents are ready
                DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.Differences, fileContentsToSend);
                Trace.WriteLine($"[Updater] Total files to send: {fileContentsToSend.Count}");

                // Serialize DataPacket
                string serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                try
                {
                    NotifyClients($"Sending files to client and waiting to recieve files from client {clientID}");
                    communicator.Send(serializedDataPacket, "ClientMetadataHandler", clientID); // Replace "Client1" with appropriate client ID
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
            }
        }

        private static void BroadcastHandler(DataPacket dataPacket)
        {
            try
            {
                // Implement BroadcastHandler logic here
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BroadcastHandler: {ex.Message}");
            }
        }

        private static void ClientFilesHandler(DataPacket dataPacket, ICommunicator communicator, string clientID)
        {
            try
            {
                NotifyClients("Recieved files from client");
                // File list
                List<FileContent> fileContentList = dataPacket.FileContentList;

                // Get files
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent != null)
                    {
                        string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                        string filePath = Path.Combine(@"C:\temp", fileContent.FileName);
                        bool status = Utils.WriteToFileFromBinary(filePath, content);

                        if (!status)
                        {
                            throw new Exception("Failed to write file");
                        }
                    }
                }

                NotifyClients("Successfully received client's files");
                Trace.WriteLine("[Updater] Successfully received client's files");

                // Broadcast client's new files to all clients
                dataPacket.DataPacketType = DataPacket.PacketType.Broadcast;

                // Serialize packet
                string serializedPacket = Utils.SerializeObject(dataPacket);

                NotifyClients("Broadcasting the new files");
                Trace.WriteLine("[Updater] Broadcasting the new files");
                try
                {
                    communicator.Send(serializedPacket, "ClientMetadataHandler", null); // Broadcast to all clients
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error in ClientFilesHandler: {ex.Message}");
            }
        }

        private static void DifferencesHandler(DataPacket dataPacket)
        {
            try
            {
                // Implement DifferencesHandler logic here
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DifferencesHandler: {ex.Message}");
            }
        }

        public void OnDataReceived(string serializedData)
        {
            semaphore.Wait(); // Wait until it's safe to enter
            try
            {
                Trace.WriteLine("[Updater] ClientMetadataHandler received data");
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
                Trace.WriteLine($"[Updater] ClientMetadataHandler detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
                NotifyClients($"Detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
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
                    NotifyClients("Detected client {clientId} disconnected");
                    Trace.WriteLine($"[Updater] ClientMetadataHandler detected client {clientId} disconnected");
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

    public static void NotifyClients(string message)
    {
        NotificationReceived?.Invoke(message);
    }
}
