using Updater;

namespace ExampleAnalyzer;

public class CodeCoverageAnalysis : ITool
{
    public int Id { get; set; }
    public string Description { get; set; }
    public Version? Version { get; set; }
    public bool IsDeprecated { get; set; }
    public string CreatorName { get; set; }
    public string CreatorEmail { get; set; }

    public CodeCoverageAnalysis()
    {
        Id = 3;
        Description = "CodeCoverageAnalysis Description";
        Version = new Version(1, 0); // Initialize Version with major and minor version
        IsDeprecated = false;
        CreatorName = "CodeCoverageAnalysis Creator";
        CreatorEmail = "creatorcca@example.com";
    }

    public Type[] ImplementedInterfaces => this.GetType().GetInterfaces();
}
