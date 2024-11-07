using Updater;

namespace UnitTests
{
    [TestClass]
    public class ToolAssemblyLoaderTests
    {
        private readonly string _emptyTestFolderPath = @"EmptyTestingFolder";
        private readonly string _testFolderPath = @"../../../TestingFolder";

        [TestInitialize]
        public void SetUp()
        {
            // Ensure the test directory exists and is clean
            if (Directory.Exists(_emptyTestFolderPath))
            {
                Directory.Delete(_emptyTestFolderPath, true);
            }
            Directory.CreateDirectory(_emptyTestFolderPath);
        }

        [TestCleanup]
        public void CleanUp()
        {
            // Clean up test files
            if (Directory.Exists(_emptyTestFolderPath))
            {
                Directory.Delete(_emptyTestFolderPath, true);
            }
        }

        [TestMethod]
        public void LoadToolsFromFolder_EmptyFolder_ReturnsEmptyDictionary()
        {
            var result = ToolAssemblyLoader.LoadToolsFromFolder(_emptyTestFolderPath);
            Assert.AreEqual(0, result.Count, "Expected empty dictionary for an empty folder.");
        }

        [TestMethod]
        public void LoadToolsFromFolder_NonDllFiles_IgnoresNonDllFiles()
        {
            File.WriteAllText(Path.Combine(_emptyTestFolderPath, "test.txt"), "This is a test file.");
            var result = ToolAssemblyLoader.LoadToolsFromFolder(_emptyTestFolderPath);
            Assert.AreEqual(0, result.Count, "Expected empty dictionary when no DLL files are present.");
        }

        [TestMethod]
        public void LoadToolsFromFolder_ValidDllWithIToolImplementation_ReturnsToolProperties()
        {
            // Construct the full path to the TestingFolder
            string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

            // Verify that the DLL exists
            string validDllPath = Path.Combine(testFolderPath, "ValidTool.dll");

            // Load tools from the folder
            var result = ToolAssemblyLoader.LoadToolsFromFolder(testFolderPath);

            // Verify that the keys exist and check their values
            Assert.IsTrue(result.TryGetValue("Id", out var ids), "Key 'Id' not found.");
            Assert.AreEqual("3", ids.FirstOrDefault(), "Expected Id was not found.");

            Assert.IsTrue(result.TryGetValue("Description", out var descriptions), "Key 'Description' not found.");
            Assert.AreEqual("CodeCoverageAnalysis Description", descriptions.FirstOrDefault(), "Expected Description was not found.");

            Assert.IsTrue(result.TryGetValue("Version", out var versions), "Key 'Version' not found.");
            Assert.AreEqual("1.0", versions.FirstOrDefault(), "Expected Version was not found.");

            Assert.IsTrue(result.TryGetValue("IsDeprecated", out var isDeprecations), "Key 'IsDeprecated' not found.");
            Assert.AreEqual("False", isDeprecations.FirstOrDefault(), "Expected IsDeprecated value was not found.");

            Assert.IsTrue(result.TryGetValue("CreatorName", out var creatorNames), "Key 'CreatorName' not found.");
            Assert.AreEqual("CodeCoverageAnalysis Creator", creatorNames.FirstOrDefault(), "Expected CreatorName was not found.");

            Assert.IsTrue(result.TryGetValue("CreatorEmail", out var creatorEmails), "Key 'CreatorEmail' not found.");
            Assert.AreEqual("creatorcca@example.com", creatorEmails.FirstOrDefault(), "Expected CreatorEmail was not found.");
        }
    }
}
