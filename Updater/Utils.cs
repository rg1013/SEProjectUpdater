﻿using System.Diagnostics;
using Networking.Serialization;

namespace Updater;

public class Utils
{

    /// <summary>
    /// Reads the content of the specified file.
    /// </summary>
    /// <param name="filePath">Path of file to read. </param>
    /// <returns>Filecontent as string, or null if file dne</returns>
    public static string? ReadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.WriteLine("File not found. Please check the path and try again.");
            return null;
        }
        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Write/Overwrite content to file
    /// </summary>
    /// <param name="filePath">Path of file</param>
    /// <param name="content">Content to write.</param>
    public static bool WriteToFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            return false;
        }
    }

    /// <summary> Serializes an object to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the serialized object.</returns>
    public static string SerializeObject<T>(T obj)
    {
        ISerializer serializer = new Serializer();
        return serializer.Serialize(obj);
    }

    /// <summary>
    /// Deserializes a string back to an object of specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="serializedData">The serialized string data.</param>
    /// <returns>An instance of the specified type.</returns>
    public static T DeserializeObject<T>(string serializedData)
    {
        ISerializer serializer = new Serializer();
        return serializer.Deserialize<T>(serializedData);
    }

    /// <summary>
    /// Generates serialized packet containing metadata of files in a directory.
    /// </summary>
    /// <returns>Serialized packet containing metadata of files in a directory.</returns>
    public static string SerializedMetadataPacket()
    {
        DirectoryMetadataGenerator metadataGenerator = new DirectoryMetadataGenerator();

        if (metadataGenerator == null)
        {
            throw new Exception("Failed to create DirectoryMetadataGenerator");
        }

        List<FileMetadata>? metadata = metadataGenerator.GetMetadata();
        if (metadata == null)
        {
            throw new Exception("Failed to get metadata");
        }

        string serializedMetadata = Utils.SerializeObject(metadata);
        Console.WriteLine(serializedMetadata);
        FileContent fileContent = new FileContent("metadata.json", serializedMetadata);
        List<FileContent> fileContents = new List<FileContent> { fileContent };

        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.Metadata, fileContents);
        Console.WriteLine(dataPacket);
        Console.WriteLine(Utils.SerializeObject(dataPacket));
        return SerializeObject(dataPacket);
    }
}
