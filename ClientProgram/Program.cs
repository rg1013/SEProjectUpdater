using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Networking;
using Networking.Communication;
using Updater;

namespace Program
{
    public class ClientProgram
    {
        static void Main(string[] args)
        {
            try
            {
                ICommunicator client = CommunicationFactory.GetCommunicator(true);

                // Starting the client
                string result = client.Start("10.32.2.232", "60091");
                if (result == "success")
                {
                    Console.WriteLine("Connected to server!");

                    // Subscribing the "ClientMetadataHandler" for handling notifications
                    client.Subscribe("ClientMetadataHandler", new ClientMetadataHandler(client));

                    string serializedMetaData = Utils.SerializedMetadataPacket();

                    // Sending data as ClientMetadataHandler
                    Console.WriteLine("Sending data as ClientMetadataHandler...");
                    client.Send(serializedMetaData, "ClientMetadataHandler", null);

                    // Waiting for some time to let data be processed
                    System.Threading.Thread.Sleep(5000);

                    Console.WriteLine("Press any key to disconnect...");
                    Console.ReadKey();

                    // Disconnecting the client
                    client.Stop();
                }
                else
                {
                    Console.WriteLine("Failed to connect to server.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClientProgram: {ex.Message}");
            }
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
                Console.WriteLine($"DataPacket -------------\n {dataPacket}");

                // Check PacketType
                switch (dataPacket.DataPacketType)
                {
                    case DataPacket.PacketType.Metadata:
                        MetadataHandler(dataPacket, communicator);
                        break;
                    case DataPacket.PacketType.Broadcast:
                        BroadcastHandler(dataPacket, communicator);
                        break;
                    case DataPacket.PacketType.ClientFiles:
                        ClientFilesHandler(dataPacket);
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
                Console.WriteLine($"Error in PacketDemultiplexer: {ex.Message}");
            }
        }

        private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MetadataHandler: {ex.Message}");
            }
        }

        private static void BroadcastHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            try
            {
                // File list
                List<FileContent> fileContentList = dataPacket.FileContentList;

                // Find the difference file
                FileContent differenceFile = fileContentList.FirstOrDefault(fc => fc.FileName.Equals("differences.xml", StringComparison.OrdinalIgnoreCase));
                if (differenceFile == null)
                {
                    throw new Exception("Difference file (differences.xml) not found");
                }

                Console.WriteLine("Inside Broadcasting!!");

                // Get files
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent.FileName.Equals("differences.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Skip the difference file
                    }

                    // Deserialize the content based on expected format
                    string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                    string filePath = Path.Combine(@"C:\temp", fileContent.FileName);
                    bool status = Utils.WriteToFileFromBinary(filePath, content);
                    if (!status)
                    {
                        throw new Exception("Failed to write file");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BroadcastHandler: {ex.Message}");
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
                Console.WriteLine($"Error in ClientFilesHandler: {ex.Message}");
            }
        }

        private static void DifferencesHandler(DataPacket dataPacket, ICommunicator communicator)
        {
            try
            {
                Console.WriteLine("INSIDE DIFFERENCE HANDLER");
                List<FileContent> fileContentList = dataPacket.FileContentList;

                foreach (FileContent fileContent in fileContentList)
                {
                    Console.WriteLine(fileContent.FileName);
                    Console.WriteLine(fileContent.ToString());
                }

                // Deserialize the 'differences' file content
                FileContent differenceFile = fileContentList[0];
                string? serializedDifferences = differenceFile.SerializedContent;
                Console.WriteLine($"SERIALIZED DIFFERENCES:\n {serializedDifferences}");

                if (serializedDifferences == null)
                {
                    throw new Exception("SerializedContent is null");
                }

                // Deserialize to List<MetadataDifference> (or appropriate type)
                List<MetadataDifference> differencesList = Utils.DeserializeObject<List<MetadataDifference>>(serializedDifferences);

                // Process additional files in the list
                foreach (FileContent fileContent in fileContentList)
                {
                    if (fileContent == differenceFile)
                    {
                        continue;
                    }

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

                    string filePath = Path.Combine(@"C:\temp", fileContent.FileName);
                    bool status = Utils.WriteToFileFromBinary(filePath, content);
                    if (!status)
                    {
                        throw new Exception("Failed to write file");
                    }
                }

                // Use the deserialized differences list to retrieve UniqueClientFiles
                List<string> filenameList = differencesList
                    .SelectMany(difference => difference.Value?.Select(fileDetail => fileDetail.FileName) ?? new List<string>())
                    .Distinct()
                    .ToList();

                Console.WriteLine("HEREEEEEE--------------------------------------------");
                Console.WriteLine(filenameList.Count);

                // Create list of FileContent to send back
                List<FileContent> fileContentToSend = new List<FileContent>();

                foreach (string filename in filenameList)
                {
                    if (filename != null)
                    {
                        string filePath = Path.Combine(@"C:\temp", filename);
                        string? content = Utils.ReadBinaryFile(filePath);

                        Console.WriteLine($"{filename}: {content}");

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
                communicator.Send(serializedDataPacket, "ClientMetadataHandler", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DifferencesHandler: {ex.Message}");
            }
        }

        public void OnDataReceived(string serializedData)
        {
            try
            {
                Console.WriteLine($"ClientMetadataHandler received data: {serializedData}");
                PacketDemultiplexer(serializedData, _communicator);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDataReceived: {ex.Message}");
            }
        }
    }
}
