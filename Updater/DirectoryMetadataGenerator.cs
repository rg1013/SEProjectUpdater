using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Diagnostics;

namespace Updater
{
    public class DirectoryMetadataGenerator
    {

        private List<FileMetadata>? _metadata;

        /// <summary>
        /// Create metadata of directory
        /// </summary>
        /// <param name="directoryPath">Path of the directory</param>
        public DirectoryMetadataGenerator(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
            }

            List<FileMetadata> metadata = CreateFileMetadata(directoryPath);
            _metadata = metadata;
        }


        /// <summary>
        /// Get metadata
        /// </summary>
        /// <returns>List of FileMetadata objects. </returns>
        public List<FileMetadata>? GetMetadata()
        {
            return _metadata;
        }
        /// <summary> Return metadata of the specified directory
        /// </summary>
        /// <param name="directoryPath">Path of directory.</param>
        /// <param name="writeToFile">bool value to write metadata to file.</param>
        /// <returns>List of FileMetadata objects in the directory.</returns>
        private static List<FileMetadata> CreateFileMetadata(string directoryPath)
        {
            List<FileMetadata> metadata = new List<FileMetadata>();
            string metadataFilePath = Path.Combine(directoryPath, "metadata.json");

            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                // Skip the metadata file itself
                if (Path.GetFileName(filePath).Equals("metadata.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                string fileHash = ComputeFileHash(filePath);
                metadata.Add(new FileMetadata
                {
                    FileName = Path.GetFileName(filePath),
                    FileHash = fileHash
                });
            }

            return metadata;
        }

        /// <summary>
        /// Computes SHA-256 hash of file. 
        /// </summary>
        /// <param name="filePath">Path of file</param>
        /// <returns>SHA-256 hash of file</returns>
        private static string ComputeFileHash(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(filePath);
            Byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

}
