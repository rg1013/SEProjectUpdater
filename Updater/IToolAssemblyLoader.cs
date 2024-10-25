namespace Updater;
public interface IToolAssemblyLoader
{
    public Dictionary<string, List<string>> LoadToolsFromFolder(string folderPath);
}
