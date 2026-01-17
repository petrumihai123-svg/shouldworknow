using System.Windows.Automation;

namespace PortableWinFormsRecorder;

public static class Selectors
{
    public static Condition BuildCondition(Target target)
    {
        var parts = new List<Condition>();

        if (!string.IsNullOrWhiteSpace(target.AutomationId))
            parts.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, target.AutomationId));

        if (!string.IsNullOrWhiteSpace(target.Name))
            parts.Add(new PropertyCondition(AutomationElement.NameProperty, target.Name));

        if (!string.IsNullOrWhiteSpace(target.ClassName))
            parts.Add(new PropertyCondition(AutomationElement.ClassNameProperty, target.ClassName));

        if (!string.IsNullOrWhiteSpace(target.ControlType))
        {
            var ct = ParseControlType(target.ControlType!);
            if (ct != null)
                parts.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ct));
        }

        if (parts.Count == 0)
            throw new ArgumentException("Target must specify at least one of: AutomationId, Name, ClassName, ControlType.");

        return parts.Count == 1 ? parts[0] : new AndCondition(parts.ToArray());
    }

    static ControlType? ParseControlType(string s)
    {
        s = s.Trim();
        return s.ToLowerInvariant() switch
        {
            "button" => ControlType.Button,
            "edit" or "textbox" => ControlType.Edit,
            "text" or "label" => ControlType.Text,
            "window" => ControlType.Window,
            "pane" => ControlType.Pane,
            "list" => ControlType.List,
            "listitem" => ControlType.ListItem,
            "checkbox" => ControlType.CheckBox,
            "combobox" => ControlType.ComboBox,
            _ => null
        };
    }
}
