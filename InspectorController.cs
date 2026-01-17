using System.Text.Json;
using System.Windows.Automation;

namespace PortableWinFormsRecorder;

public sealed class InspectorController
{
    readonly HighlightOverlay _highlight = new();
    readonly Timer _timer = new() { Interval = 60 };

    public bool IsRunning { get; private set; }

    public InspectorController()
    {
        _timer.Tick += (_, __) => Tick();
    }

    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        _timer.Start();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _timer.Stop();
        _highlight.HideRect();
    }

    void Tick()
    {
        var p = Cursor.Position;
        try
        {
            var el = AutomationElement.FromPoint(new System.Windows.Point(p.X, p.Y));
            if (el == null) { _highlight.HideRect(); return; }

            var rect = el.Current.BoundingRectangle;
            if (rect.IsEmpty) { _highlight.HideRect(); return; }

            var r = Rectangle.FromLTRB((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
            _highlight.ShowRect(WindowCapture.InflateSafe(r, 2));

            // Ctrl+Shift+C copies a Target JSON to clipboard
            if (Win32Hooks.IsKeyDown(Win32Hooks.VK_CONTROL) && Win32Hooks.IsKeyDown(Win32Hooks.VK_SHIFT)
                && (Control.ModifierKeys & Keys.Control) == Keys.Control
                && (Control.ModifierKeys & Keys.Shift) == Keys.Shift
                && (Control.IsKeyLocked(Keys.CapsLock) == false))
            {
                // Don’t auto-trigger on every tick; require the C key down
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift
                    && (GetAsyncKeyDown(Keys.C)))
                {
                    var t = UiAutomationUtil.BuildTargetFromElement(el);
                    var json = JsonSerializer.Serialize(t, JsonUtil.Options);
                    Clipboard.SetText(json);
                }
            }
        }
        catch
        {
            _highlight.HideRect();
        }
    }

    static bool GetAsyncKeyDown(Keys key)
    {
        // Lightweight polling to avoid another hook in inspector
        return (Win32Hooks.GetKeyState((int)key) & 0x8000) != 0;
    }
}
