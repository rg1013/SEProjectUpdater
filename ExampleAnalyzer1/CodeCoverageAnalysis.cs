using Updater;

namespace ExampleAnalyzer1;

public class CodeCoverageAnalysis : ITool
{
    public int Id { get; set; }
    public string Description { get; set; }
    public float? Version { get; set; }
    public bool IsDeprecated { get; set; }
    public string CreatorName { get; set; }
    public string CreatorEmail { get; set; }

    public CodeCoverageAnalysis()
    {
        Id = 3;
        Description = "CodeCoverageAnalysis Description";
        Version = 1.0f;
        IsDeprecated = false;
        CreatorName = "CodeCoverageAnalysis Creator";
        CreatorEmail = "creatorcca@example.com";
    }

    public Type[] ImplementedInterfaces => this.GetType().GetInterfaces();
}
