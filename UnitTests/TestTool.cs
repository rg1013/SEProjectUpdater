using ViewModels;

namespace UnitTests;

[TestClass]
public class TestTool
{
    private Tool? _tool;

    [TestInitialize]
    public void Setup()
    {
        _tool = new Tool();
    }

    [TestMethod]
    public void IDShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.ID))
            {
                eventRaised = true;
            }
        };

        _tool.ID = "123";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for ID.");
        Assert.AreEqual("123", _tool.ID, "ID was not set correctly.");
    }

    [TestMethod]
    public void VersionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Version))
            {
                eventRaised = true;
            }
        };

        _tool.Version = "1.0.0";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Version.");
        Assert.AreEqual("1.0.0", _tool.Version, "Version was not set correctly.");
    }

    [TestMethod]
    public void DescriptionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Description))
            {
                eventRaised = true;
            }
        };

        _tool.Description = "Sample tool description.";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Description.");
        Assert.AreEqual("Sample tool description.", _tool.Description, "Description was not set correctly.");
    }

    [TestMethod]
    public void DeprecatedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Deprecated))
            {
                eventRaised = true;
            }
        };

        _tool.Deprecated = "Yes";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Deprecated.");
        Assert.AreEqual("Yes", _tool.Deprecated, "Deprecated was not set correctly.");
    }

    [TestMethod]
    public void CreatedByShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.CreatedBy))
            {
                eventRaised = true;
            }
        };

        _tool.CreatedBy = "Jane Doe";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for CreatedBy.");
        Assert.AreEqual("Jane Doe", _tool.CreatedBy, "CreatedBy was not set correctly.");
    }

    [TestMethod]
    public void PropertyChangedEventIsNotRaisedWhenSettingSameValue()
    {
        _tool.ID = "123"; // Setting initial value
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.ID))
            {
                eventRaised = true;
            }
        };
        _tool.ID = "123"; // Setting the same value again

        Assert.IsFalse(eventRaised, "PropertyChanged event should not be raised when setting the same value.");
    }
}