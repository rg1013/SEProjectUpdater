using System.Diagnostics;
using Networking.Serialization;

namespace Updater;

public class Utils
{

    /// <summary>
    /// Reads the content of the specified file.
    /// </summary>
    /// <param name="filePath">Path of file to read. </param>
    /// <returns>Filecontent as string, or null if file dne</returns>
    static string? ReadFile(string filePath)
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
    static bool WriteToFile(string filePath, string content)
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


    /// <summary>
    /// Serializes an object to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the serialized object.</returns>
    static string SerializeObject<T>(T obj)
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
    static T DeserializeObject<T>(string serializedData)
    {
        ISerializer serializer = new Serializer();
        return serializer.Deserialize<T>(serializedData);
    }

}
