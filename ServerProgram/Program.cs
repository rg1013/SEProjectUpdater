using System;
using Networking.Communication;
using System.Net.Sockets;
using Networking;
using Updater;

namespace Program;

public class ServerProgram
{
    static void Main(string[] args)
    {
        //CommunicatorServer server = new CommunicatorServer();
        ICommunicator server = CommunicationFactory.GetCommunicator(false);

        // Starting the server
        string result = server.Start("127.0.0.1", "12345");
        Console.WriteLine($"Server started on {result}");

        // Subscribing the "ClientMetadataHandler" for handling notifications
        server.Subscribe("ClientMetadataHandler", new ClientMetadataHandler());
        server.Subscribe("ModuleB", new ModuleBHandler());

        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        // Stop the server when user presses a key
        server.Stop();
    }
}

public class ClientMetadataHandler : INotificationHandler
{
    public void OnDataReceived(string serializedData)
    {
        Console.WriteLine($"ClientMetadataHandler received data: {serializedData}");
        try
        {
            DataPacket deserializedData = Utils.DeserializeObject<DataPacket>(serializedData);
            if (deserializedData == null)
            {
                Console.WriteLine("Deserialized data is null.");
            }
            else
            {
                Console.WriteLine(deserializedData.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
        }

    }

    public void OnClientJoined(TcpClient socket)
    {
        Console.WriteLine($"ClientMetadataHandler detected new client connection: {socket.Client.RemoteEndPoint}");
    }

    public void OnClientLeft(string clientId)
    {
        Console.WriteLine($"ClientMetadataHandler detected client {clientId} disconnected");
    }
}

public class ModuleBHandler : INotificationHandler
{
    public void OnDataReceived(string serializedData)
    {
        Console.WriteLine($"ModuleB received data: {serializedData}");
    }

    public void OnClientJoined(TcpClient socket)
    {
        Console.WriteLine($"ModuleB detected new client connection: {socket.Client.RemoteEndPoint}");
    }

    public void OnClientLeft(string clientId)
    {
        Console.WriteLine($"ModuleB detected client {clientId} disconnected");
    }
}