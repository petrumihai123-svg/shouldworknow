using System.Text.Json.Serialization;

namespace PortableWinFormsRecorder;

public sealed class Script
{
    public int Version { get; set; } = 1;
    public List<Step> Steps { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionKind
{
    Click,
    SetText,
    WaitMs,

    AssertExists,
    AssertTextEquals,
    AssertTextContains
}

public sealed class Step
{
    public ActionKind Action { get; set; }
    public Target? Target { get; set; }
    public string? Value { get; set; }
    public int? Ms { get; set; }
}

public sealed class Target
{
    // Any of these may be used to resolve element
    public string? AutomationId { get; set; }
    public string? Name { get; set; }
    public string? ClassName { get; set; }
    public string? ControlType { get; set; } // e.g. "Button", "Edit", "Text"
    public string? Process { get; set; }     // process name (optional)
}
