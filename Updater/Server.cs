using Networking.Communication;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Updater;

public class Server
{
    static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Allow one client at a time

    static void Main(string[] args)
    {
        try
        {
            ICommunicator server = CommunicationFactory.GetCommunicator(false);

            // Starting the server
            string result = server.Start("10.32.2.232", "60091");
            Console.WriteLine($"Server started on {result}");

            // Subscribing the "ClientMetadataHandler" for handling notifications
            server.Subscribe("ClientMetadataHandler", new ClientMetadataHandler(server));

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            // Stop the server when user presses a key
            server.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in server: {ex.Message}");
        }
    }


    public class ClientMetadataHandler : INotificationHandler
    {
        private readonly ICommunicator _communicator;

        public ClientMetadataHandler(ICommunicator communicator)
        {
            _communicator = communicator;
        }

        public static void PacketDemultiplexer(string serializedData, ICommunicator communicator)
        {
            try
            {
                // Deserialize data
                DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

                // Check PacketType
                switch (dataPacket.DataPacketType)
                {
                    case DataPacket.PacketType.Metadata:
                        MetadataHandler(dataPacket, communicator);
                        break;
                    case DataPacket.PacketType.Broadcast:
                        BroadcastHandler(dataPacket);
                        break;
                    case DataPacket.PacketType.ClientFiles:
                        ClientFilesHandler(dataPacket, communicator);
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

        private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator)
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

                Console.WriteLine(serializedContent ?? "Serialized content is null");

                // Deserialize the client metadata
                List<FileMetadata>? metadataClient = Utils.DeserializeObject<List<FileMetadata>>(serializedContent);
                if (metadataClient == null)
                {
                    throw new Exception("Deserialized client metadata is null");
                }
                Console.WriteLine("Metadata from client received");

                // Generate metadata of server
                List<FileMetadata>? metadataServer = new DirectoryMetadataGenerator(@"C:\received").GetMetadata();
                if (metadataServer == null)
                {
                    throw new Exception("Metadata server is null");
                }
                Console.WriteLine("Metadata from server generated");

                // Compare metadata and get differences
                DirectoryMetadataComparer comparerInstance = new DirectoryMetadataComparer(metadataServer, metadataClient);
                var differences = comparerInstance.Differences;

                // Serialize and save differences to C:\temp\ folder
                string serializedDifferences = Utils.SerializeObject(differences);
                string tempFilePath = @"C:\received\differences.xml";

                if (string.IsNullOrEmpty(serializedDifferences))
                {
                    Console.WriteLine("Serialization of differences failed or resulted in an empty string.");
                    return; // Exit if serialization fails
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
                    File.WriteAllText(tempFilePath, serializedDifferences);
                    Console.WriteLine($"Differences file saved to {tempFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving differences file: {ex.Message}");
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
                    string filePath = Path.Combine(@"C:\received", filename);
                    string? content = Utils.ReadBinaryFile(filePath);

                    if (content == null)
                    {
                        Console.WriteLine($"Warning: Content of file {filename} is null, skipping.");
                        continue; // Skip to the next file instead of throwing an exception
                    }

                    Console.WriteLine($"Content length of {filename}: {content.Length}");

                    // Serialize file content and create FileContent object
                    string serializedFileContent = Utils.SerializeObject(content);
                    if (string.IsNullOrEmpty(serializedFileContent))
                    {
                        Console.WriteLine($"Warning: Serialized content for {filename} is null or empty.");
                        continue; // Skip to next file if serialization fails
                    }

                    FileContent fileContentToSend = new FileContent(filename, serializedFileContent);
                    fileContentsToSend.Add(fileContentToSend);
                }

                // Create DataPacket after all FileContents are ready
                DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.Differences, fileContentsToSend);
                Console.WriteLine($"Total files to send: {fileContentsToSend.Count}");
                foreach (var filecontent in fileContentsToSend)
                {
                    Console.WriteLine($"File: {filecontent.FileName}, Serialized Length: {filecontent.SerializedContent?.Length}");
                }

                // Serialize DataPacket
                string serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                try
                {
                    communicator.Send(serializedDataPacket, "ClientMetadataHandler", "Client1");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to client: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to client: {ex.Message}");
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

        private static void ClientFilesHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            try
            {
                // File list
                List<FileContent> fileContentList = dataPacket.FileContentList;

                // Get files
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent != null)
                    {
                        string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                        string filePath = Path.Combine(@"C:\received", fileContent.FileName);
                        bool status = Utils.WriteToFileFromBinary(filePath, content);

                        if (!status)
                        {
                            throw new Exception("Failed to write file");
                        }
                    }
                }
                Console.WriteLine("Successfully recieved client's files");

                // Broadcast client's new files to all clients
                dataPacket.DataPacketType = DataPacket.PacketType.Broadcast;

                // Serialize packet
                string serializedPacket = Utils.SerializeObject(dataPacket);

                Console.WriteLine("BROADCASTING!!!");
                try
                {
                    communicator.Send(serializedPacket, "ClientMetadataHandler", null);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClientFilesHandler: {ex.Message}");
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
                Console.WriteLine("ClientMetadataHandler received data");
                DataPacket deserializedData = Utils.DeserializeObject<DataPacket>(serializedData);
                if (deserializedData == null)
                {
                    Console.WriteLine("Deserialized data is null.");
                }
                else
                {
                    Console.WriteLine("Read Successfully");
                    PacketDemultiplexer(serializedData, _communicator);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization failed: {ex.Message}");
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
                Console.WriteLine($"ClientMetadataHandler detected new client connection: {socket.Client.RemoteEndPoint}");
                _communicator.AddClient("Client1", socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnClientJoined: {ex.Message}");
            }
        }

        public void OnClientLeft(string clientId)
        {
            try
            {
                Console.WriteLine($"ClientMetadataHandler detected client {clientId} disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnClientLeft: {ex.Message}");
            }
        }
    }
    public event Action<string> OnNotificationReceived; // Event to notify the view model

    public void NotifyClients(string message)
    {
        // Logic for notifying clients...
        OnNotificationReceived?.Invoke(message); // Raise the event
    }
}
