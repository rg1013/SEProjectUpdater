using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater;

public class FileContent
{
    public string? FileName { get; set; }
    public string? SerializedContent { get; set; }

    public FileContent(string? fileName, string? serializedContent)
    {
        FileName = fileName;
        SerializedContent = serializedContent;
    }

    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, Content Length: {SerializedContent?.Length ?? 0}";
    }
}
