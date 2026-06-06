namespace Accessly.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public bool Enabled { get; set; } = true;

    /// <summary>"Fake" (default, offline) or "OpenAiCompatible" (requires BaseUrl + ApiKey).</summary>
    public string Provider { get; set; } = "Fake";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}
