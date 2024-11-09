/******************************************************************************
* Filename    = Utils.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = 
*****************************************************************************/

using System.Diagnostics;
using System.Text.RegularExpressions;
using Networking.Serialization;

namespace Updater;

public class Utils
{
    public static List<FileContent> ExistingFiles { get; set; } = new List<FileContent>();

    /// <summary>
    /// Reads the content of the specified file.
    /// </summary>
    /// <param name="filePath">Path of file to read. </param>
    /// <returns>Filecontent as string, or null if file dne</returns>
    public static string? ReadBinaryFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.WriteLine("File not found. Please check the path and try again.");
            return null;
        }

        // Read all bytes from the file
        byte[] byteArray = File.ReadAllBytes(filePath);

        // Convert byte array to a base64 string
        return Convert.ToBase64String(byteArray);
    }


    /// <summary>
    /// Write/Overwrite content to file
    /// </summary>
    /// <param name="filePath">Path of file</param>
    /// <param name="content">Content to write.</param>
    public static bool WriteToFileFromBinary(string filePath, string content)
    {
        try
        {
            byte[] data;

            // Check if the content is in base64 format by attempting to decode it
            try
            {
                data = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                // If it's not base64, write as a regular string
                File.WriteAllText(filePath, content);
                return true;
            }

            // If decoding to byte array is successful, write as binary
            File.WriteAllBytes(filePath, data);
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
        try
        {
            ISerializer serializer = new Serializer();
            return serializer.Serialize(obj);
        }
        catch (Exception ex) { 
            Console.WriteLine(ex.ToString());
            return "";
        }
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
        FileContent fileContent = new FileContent("metadata.json", serializedMetadata);
        List<FileContent> fileContents = new List<FileContent> { fileContent };

        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.Metadata, fileContents);
        return SerializeObject(dataPacket);
    }

    /// <summary>
    /// Get the get version of the file.
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FormatException"></exception>
    public static string GetCurrentVersion(string filepath)
    {
        try
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"File not found at {filepath}");
            }

            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line = reader.ReadLine();

                if (line != null && line.StartsWith("Version:"))
                {
                    return line.Substring("Version".Length).Trim();
                }
                else
                {
                    throw new FormatException("Version information not found in the expected format");
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCurrentVersion: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Keeps filenames in versioned format
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="newFileVersion"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string StandardizeFileName(string? fileName, string newFileVersion)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
        }

        if (string.IsNullOrEmpty(newFileVersion))
        {
            throw new ArgumentException("New file version cannot be null or empty", nameof(newFileVersion));
        }

        // Extract file name and extension
        string fileExtension = Path.GetExtension(fileName);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Regex pattern to detect a version suffix (e.g., _v1.0)
        string versionPattern = @"(_v\d+(\.\d+)*)$";

        // Check if the filename already has a version suffix and remove it if present
        if (Regex.IsMatch(fileNameWithoutExtension, versionPattern))
        {
            fileNameWithoutExtension = Regex.Replace(fileNameWithoutExtension, versionPattern, "");
        }

        // Append the new version to the filename in the format _v<newVersion>
        string standardizedFileName = $"{fileNameWithoutExtension}_v{newFileVersion}{fileExtension}";

        return standardizedFileName;
    }

    /// <summary>
    /// Finds a file in the existing files list that has the same content as the target file, 
    /// regardless of the file name or version.
    /// </summary>
    /// <param name="targetFile">The file content to compare with existing files.</param>
    /// <returns>A similar FileContent if found, otherwise null.</returns>
    public static FileContent? FindSimilarContentFile(FileContent targetFile)
    {
        if (targetFile.FileName == null || targetFile.SerializedContent == null)
        {
            throw new ArgumentException("Target file must have a name and content.");
        }

        // If there are multiple similar files, prefer an exact content match
        foreach (var file in ExistingFiles)
        {
            if (file.SerializedContent == targetFile.SerializedContent)
            {
                return file;
            }
        }

        // Return the first similar file by name if no content matches are found
        return null;
    }

    /// <summary>
    /// Removes the version suffix (e.g., "_v1.0") from the file name.
    /// </summary>
    /// <param name="fileName">The file name to process.</param>
    /// <returns>The file name without the version suffix.</returns>
    public static string RemoveVersionSuffix(string fileName)
    {
        // Regex pattern to detect and remove a version suffix, e.g., _v1.0 or _v1
        string versionPattern = @"(_v\d+(\.\d+)*)$";
        return Regex.Replace(Path.GetFileNameWithoutExtension(fileName), versionPattern, "");
    }
}
