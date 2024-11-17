﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;

namespace Updater.Tests;

[TestClass]
public class FileContentTests
{
    /// <summary>
    /// Verifies that the default constructor sets both FileName and SerializedContent to null.
    /// </summary>
    [TestMethod]
    public void Test_FileContent_DefaultConstructor()
    {
        // Arrange & Act
        var fileContent = new FileContent();

        // Assert
        Assert.IsNull(fileContent.FileName);
        Assert.IsNull(fileContent.SerializedContent);
    }

    /// <summary>
    /// Verifies that the constructor sets the FileName and SerializedContent correctly when valid values are provided.
    /// </summary>
    [TestMethod]
    public void Test_FileContent_Constructor_WithValidParams()
    {
        // Arrange
        var fileName = "example.txt";
        var serializedContent = "Some content";

        // Act
        var fileContent = new FileContent(fileName, serializedContent);

        // Assert
        Assert.AreEqual(fileName, fileContent.FileName);
        Assert.AreEqual(serializedContent, fileContent.SerializedContent);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both properties are null.
    /// </summary>
    [TestMethod]
    public void Test_FileContent_ToString_ForDefaultConstructor()
    {
        // Arrange
        var fileContent = new FileContent();

        // Act
        var result = fileContent.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, Content Length: 0", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both FileName and SerializedContent are not null.
    /// </summary>
    [TestMethod]
    public void Test_FileContent_ToString_BothPropertiesNotNull()
    {
        // Arrange
        var fileContent = new FileContent("example.txt", "Some content");

        // Act
        var result = fileContent.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, Content Length: 12", result);
    }
}

