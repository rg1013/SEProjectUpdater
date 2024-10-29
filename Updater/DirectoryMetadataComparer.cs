using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater;

public class DirectoryMetadataComparer
{
    private Dictionary<int, List<object>>? _differences;
    private List<string> _uniqueServerFiles = new List<string>();
    private List<string> _uniqueClientFiles = new List<string>();


    /// <summary>
    /// Initialize new instance. 
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="metadataB">Dir. B's metadata</param>
    public DirectoryMetadataComparer(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
    {
        _differences = CompareMetadata(metadataA, metadataB);
    }

    public List<string> GetUniqueServerFiles()
    {
        return _uniqueServerFiles;
    }


    public List<string> GetUniqueClientFiles()
    {
        return _uniqueServerFiles;
    }

    /// <summary>
    /// Get _differences
    /// </summary>
    /// <returns>Dictionary containing differences between metadata of dir. A and B</returns>
    public Dictionary<int, List<object>>? GetDifferences()
    {
        return _differences;
    }


    /// <summary>
    /// Compares and generate difference between a dir. metadata pair
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="metadataB">Dir. B's metadata</param>
    /// <returns>Dictionary containing differences, </returns>
    private Dictionary<int, List<object>> CompareMetadata(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
    {
        Dictionary<int, List<object>> differences = new Dictionary<int, List<object>>
    {
        { -1, new List<object>() }, // In B but not in A
        { 0, new List<object>() },  // Files with same hash but different names
        { 1, new List<object>() }   // In A but not in B
    };

        Dictionary<string, string> hashToFileA = CreateHashToFileDictionary(metadataA);
        Dictionary<string, string> hashToFileB = CreateHashToFileDictionary(metadataB);

        CheckForRenamesAndMissingFiles(metadataB, hashToFileA, differences);
        CheckForOnlyInAFiles(metadataA, hashToFileB, differences);

        return differences;
    }


    /// <summary>
    /// Create a map from filehash to filename
    /// </summary>
    /// <param name="metadata">list of metadata.</param>
    /// <returns>Dictionary containing mapping.</returns>
    private static Dictionary<string, string> CreateHashToFileDictionary(List<FileMetadata> metadata)
    {
        var hashToFile = new Dictionary<string, string>();
        foreach (var file in metadata)
        {
            hashToFile[file.FileHash] = file.FileName;
        }
        return hashToFile;
    }


    /// <summary>
    /// Checks for files in directory B that have been renamed or missing in directory A.
    /// </summary>
    /// <param name="metadataB">Dir. B's metadata.</param>
    /// <param name="hashToFileA">Dir. A's Hash to file map</param>
    /// <param name="differences">differences dictionary</param>
    /// <returns> Differences dictionary</returns>
    private void CheckForRenamesAndMissingFiles(List<FileMetadata> metadataB, Dictionary<string, string> hashToFileA, Dictionary<int, List<object>> differences)
    {
        foreach (FileMetadata fileB in metadataB)
        {
            if (hashToFileA.ContainsKey(fileB.FileHash))
            {
                if (hashToFileA[fileB.FileHash] != fileB.FileName)
                {
                    differences[0].Add(new Dictionary<string, string>
                {
                    { "RenameFrom", fileB.FileName },
                    { "RenameTo", hashToFileA[fileB.FileHash] },
                    { "FileHash", fileB.FileHash }
                });
                }
            }
            else
            {
                differences[-1].Add(new Dictionary<string, string>
            {
                { "FileName", fileB.FileName },
                { "FileHash", fileB.FileHash }
            });
                _uniqueClientFiles.Add(fileB.FileName);
            }
        }
    }


    /// <summary>
    /// Checks for files in directory A that are missing in directory B.
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="hashToFileB">Dir. B's Hash to file map</param>
    /// <param name="differences">Differences dictionary</param>
    /// <returns> Differences dictionary</returns>
    private void CheckForOnlyInAFiles(List<FileMetadata> metadataA, Dictionary<string, string> hashToFileB, Dictionary<int, List<object>> differences)
    {
        foreach (FileMetadata fileA in metadataA)
        {
            if (!hashToFileB.ContainsKey(fileA.FileHash))
            {
                differences[1].Add(new Dictionary<string, string>
            {
                { "FileName", fileA.FileName },
                { "FileHash", fileA.FileHash }
            });
                _uniqueServerFiles.Add(fileA.FileName);
            }
        }
    }
}
