using System;
using Networking.Communication;
using Networking;
using Updater;

namespace Program;

public class ClientProgram
{
    static void Main(string[] args)
    {
        //CommunicatorClient client = new CommunicatorClient();
        ICommunicator client = CommunicationFactory.GetCommunicator(true);

        // Starting the client
        string result = client.Start("10.32.2.232", "59429");
        if (result == "success")
        {
            Console.WriteLine("Connected to server!");

            // Subscribing the "ModuleA" for handling notifications
            client.Subscribe("ClientMetadataHandler", new ClientMetadataHandler());
            client.Subscribe("ModuleB", new ModuleBHandler());

            string serializedMetaData = Utils.SerializedMetadataPacket();

            // Sending data as ClientMetadataHandler
            Console.WriteLine("Sending data as ClientMetadataHandler...");
            Console.WriteLine(serializedMetaData);
            client.Send(serializedMetaData, "ClientMetadataHandler", null);

            // Waiting for some time to let data be processed
            System.Threading.Thread.Sleep(10000);

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
}

public class ClientMetadataHandler : INotificationHandler
{
    public void OnDataReceived(string serializedData)
    {
        Console.WriteLine($"ClientMetadataHandler received data: {serializedData}");
    }
}

public class ModuleBHandler : INotificationHandler
{
    public void OnDataReceived(string serializedData)
    {
        Console.WriteLine($"ModuleB received data: {serializedData}");
    }
}