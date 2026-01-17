using System.Diagnostics;
using System.Windows.Automation;

namespace PortableWinFormsRecorder;

public static class UiAutomationUtil
{
    public static Target BuildTargetFromElement(AutomationElement el)
    {
        var t = new Target
        {
            AutomationId = Safe(() => el.Current.AutomationId),
            Name = Safe(() => el.Current.Name),
            ClassName = Safe(() => el.Current.ClassName),
            ControlType = Safe(() => el.Current.ControlType?.ProgrammaticName?.Replace("ControlType.", "")),
        };

        try
        {
            var pid = el.Current.ProcessId;
            if (pid > 0)
            {
                var p = Process.GetProcessById(pid);
                t.Process = p.ProcessName;
            }
        }
        catch { /* ignore */ }

        // Trim empties
        if (string.IsNullOrWhiteSpace(t.AutomationId)) t.AutomationId = null;
        if (string.IsNullOrWhiteSpace(t.Name)) t.Name = null;
        if (string.IsNullOrWhiteSpace(t.ClassName)) t.ClassName = null;
        if (string.IsNullOrWhiteSpace(t.ControlType)) t.ControlType = null;
        if (string.IsNullOrWhiteSpace(t.Process)) t.Process = null;

        return t;
    }

    public static Rectangle? GetBoundingRect(AutomationElement el)
    {
        try
        {
            var r = el.Current.BoundingRectangle;
            if (r.IsEmpty) return null;
            return Rectangle.FromLTRB((int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom);
        }
        catch { return null; }
    }

    static string? Safe(Func<string?> f)
    {
        try { return f(); } catch { return null; }
    }
}
