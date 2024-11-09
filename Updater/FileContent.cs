/******************************************************************************
* Filename    = FileContent.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = This class represents the content and metadata of a file, including its name, serialized content, and version.
* 
*****************************************************************************/

using System;
using System.Xml.Serialization;

namespace Updater;

[Serializable]
public class FileContent
{
    // Parameterless constructor is required for XML serialization
    public FileContent() {
        Version = "1.0";
    }

    public FileContent(string? fileName, string? serializedContent, string version = "1.0")
    {
        FileName = fileName;
        SerializedContent = serializedContent;
        Version = version;
    }

    [XmlElement("FileName")]
    public string? FileName { get; set; }

    [XmlElement("SerializedContent")]
    public string? SerializedContent { get; set; }

    [XmlElement("Version")]
    public string? Version { get; set; } = "1.0";
    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, Content Length: {SerializedContent?.Length ?? 0}, Version: {Version}";
    }


    public override bool Equals(object? obj)
    {
        if (obj is FileContent other)
        {
            return FileName == other.FileName &&
                   SerializedContent == other.SerializedContent &&
                   Version == other.Version;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FileName, SerializedContent, Version);
    }
}
