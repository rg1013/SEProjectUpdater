using UpdaterViewModel;

namespace UnitTests;

[TestClass]
public class TestFileChangeNotifier
{
    private FileChangeNotifier _fileMonitor;

    [TestInitialize]
    public void Setup()
    {
        _fileMonitor = new FileChangeNotifier();
    }

    [TestMethod]
    public void TestFileCreatedUpdateMessageStatus()
    {
        // Arrange
        string testFilePath = @"C:\Users\harik\Downloads\testfile.txt";

        // Act: Simulate file creation event using reflection to call private method
        _fileMonitor.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(testFilePath), Path.GetFileName(testFilePath)) });

        // Simulate timer elapse
        Thread.Sleep(1100); // Consider increasing sleep time slightly for reliability

        // Assert
        Assert.AreEqual("Files created: testfile.txt", _fileMonitor.MessageStatus.TrimEnd());
    }

    [TestMethod]
    public void TestFileDeletedUpdateMessageStatus()
    {
        // Arrange
        string testFilePath = @"C:\Users\harik\Downloads\testfile.txt";

        // Act: Simulate file deletion event using reflection
        _fileMonitor.GetType()
            .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(testFilePath), Path.GetFileName(testFilePath)) });

        // Simulate timer elapse
        Thread.Sleep(1100);

        // Assert
        Assert.AreEqual("Files removed: testfile.txt", _fileMonitor.MessageStatus.TrimEnd());
    }

    [TestMethod]
    public void TestMultipleFilesUpdateMessageStatus()
    {
        // Arrange
        string file1 = @"C:\Users\harik\Downloads\file1.txt";
        string file2 = @"C:\Users\harik\Downloads\file2.txt";
        string deletedFile = @"C:\Users\harik\Downloads\deletedfile.txt";

        // Act: Simulate multiple file events using reflection
        _fileMonitor.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file1), Path.GetFileName(file1)) });

        _fileMonitor.GetType()
            .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file2), Path.GetFileName(file2)) });

        _fileMonitor.GetType()
            .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(deletedFile), Path.GetFileName(deletedFile)) });

        // Simulate timer elapse
        Thread.Sleep(1100);

        // Assert
        // Assert
        Assert.AreEqual(
            "Files created: file1.txt, file2.txt\nFiles removed: deletedfile.txt".Replace("\r\n", "\n").Trim(),
            _fileMonitor.MessageStatus.Replace("\r\n", "\n").Trim()
        );



    }
}
