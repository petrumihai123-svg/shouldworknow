using System.Diagnostics;
using System.Windows.Automation;

namespace PortableWinFormsRecorder;

public sealed class Runner
{
    public void Execute(Script script, IReadOnlyDictionary<string, string> data)
    {
        foreach (var step in script.Steps)
            ExecuteStep(step, data);
    }

    void ExecuteStep(Step step, IReadOnlyDictionary<string, string> data)
    {
        switch (step.Action)
        {
            case ActionKind.WaitMs:
                Thread.Sleep(step.Ms ?? 0);
                return;

            case ActionKind.Click:
                {
                    var el = Resolve(step.Target);
                    InvokeClick(el);
                    return;
                }

            case ActionKind.SetText:
                {
                    var el = Resolve(step.Target);
                    var text = Templating.Apply(step.Value, data);
                    SetText(el, text);
                    return;
                }

            case ActionKind.AssertExists:
                _ = Resolve(step.Target);
                return;

            case ActionKind.AssertTextEquals:
                {
                    var el = Resolve(step.Target);
                    var actual = ReadText(el);
                    var expected = Templating.Apply(step.Value, data);
                    if (!string.Equals(actual, expected, StringComparison.Ordinal))
                        throw new InvalidOperationException($"AssertTextEquals failed. Expected='{expected}', Actual='{actual}'");
                    return;
                }

            case ActionKind.AssertTextContains:
                {
                    var el = Resolve(step.Target);
                    var actual = ReadText(el);
                    var expected = Templating.Apply(step.Value, data);
                    if (actual == null || !actual.Contains(expected ?? "", StringComparison.Ordinal))
                        throw new InvalidOperationException($"AssertTextContains failed. Expected contains='{expected}', Actual='{actual}'");
                    return;
                }

            default:
                throw new NotSupportedException($"Unsupported action: {step.Action}");
        }
    }

    static AutomationElement Resolve(Target? target)
    {
        if (target == null)
            throw new ArgumentException("Step target is required.");

        AutomationElement root = AutomationElement.RootElement;

        // Optional: narrow to process
        if (!string.IsNullOrWhiteSpace(target.Process))
        {
            var p = Process.GetProcessesByName(target.Process).FirstOrDefault();
            if (p == null) throw new InvalidOperationException($"Process not found: {target.Process}");
            root = AutomationElement.FromHandle(p.MainWindowHandle);
        }

        var cond = Selectors.BuildCondition(target);
        var el = root.FindFirst(TreeScope.Descendants, cond);
        if (el == null)
        {
            throw new InvalidOperationException(
                $"Element not found. automationId='{target.AutomationId}', name='{target.Name}', class='{target.ClassName}', type='{target.ControlType}', process='{target.Process}'");
        }
        return el;
    }

    static void InvokeClick(AutomationElement el)
    {
        if (el.TryGetCurrentPattern(InvokePattern.Pattern, out var p) && p is InvokePattern inv)
        {
            inv.Invoke();
            return;
        }

        // Fallback: select if possible
        if (el.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var s) && s is SelectionItemPattern sel)
        {
            sel.Select();
            return;
        }

        throw new NotSupportedException("Element does not support Invoke/SelectionItem patterns for click.");
    }

    static void SetText(AutomationElement el, string text)
    {
        if (el.TryGetCurrentPattern(ValuePattern.Pattern, out var p) && p is ValuePattern vp)
        {
            vp.SetValue(text);
            return;
        }

        throw new NotSupportedException("Element does not support ValuePattern for SetText.");
    }

    static string ReadText(AutomationElement el)
    {
        // Prefer ValuePattern
        if (el.TryGetCurrentPattern(ValuePattern.Pattern, out var p) && p is ValuePattern vp)
            return vp.Current.Value ?? "";

        // Fallback: name
        return el.Current.Name ?? "";
    }
}
