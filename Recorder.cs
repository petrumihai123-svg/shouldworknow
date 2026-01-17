using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace PortableWinFormsRecorder;

public sealed class Recorder
{
    Script? _script;
    bool _recording;

    IntPtr _mouseHook = IntPtr.Zero;
    Win32Hooks.HookProc? _mouseProc;

    readonly HighlightOverlay _highlight = new();
    readonly RecordingIndicatorOverlay _recOverlay = new();

    // Hover highlight while recording
    readonly System.Windows.Forms.Timer _hoverTimer = new() { Interval = 50 };

    // Settings
    public bool CaptureScreenshots { get; set; } = true;

    // Where to put assets (if CaptureScreenshots)
    public string? ScriptPathForAssets { get; set; }

    // Optional: ignore clicks on our own overlay windows
    public Func<IntPtr, bool>? IsWindowOurs { get; set; }

    public bool IsRecording => _recording;

    public Recorder()
    {
        _hoverTimer.Tick += (_, __) => HoverTick();
    }

    public void Start(Script script)
    {
        _script = script ?? throw new ArgumentNullException(nameof(script));
        _recording = true;

        _recOverlay.SetText("● REC (click to record)");
        _recOverlay.Show();

        _hoverTimer.Start();

        InstallMouseHook();
    }

    public void Stop()
    {
        _recording = false;
        _script = null;

        _hoverTimer.Stop();
        _highlight.HideRect();
        _recOverlay.Hide();

        RemoveMouseHook();
    }

    void InstallMouseHook()
    {
        if (_mouseHook != IntPtr.Zero) return;

        _mouseProc = MouseHookCallback;

        // Low-level hook: hMod should be module handle; passing current module works
        var hMod = Win32Hooks.GetModuleHandle(null);
        _mouseHook = Win32Hooks.SetWindowsHookEx(Win32Hooks.WH_MOUSE_LL, _mouseProc, hMod, 0);

        if (_mouseHook == IntPtr.Zero)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to set mouse hook.");
    }

    void RemoveMouseHook()
    {
        if (_mouseHook == IntPtr.Zero) return;
        Win32Hooks.UnhookWindowsHookEx(_mouseHook);
        _mouseHook = IntPtr.Zero;
        _mouseProc = null;
    }

    void HoverTick()
    {
        if (!_recording) return;

        var p = Cursor.Position;
        try
        {
            var el = AutomationElement.FromPoint(new System.Windows.Point(p.X, p.Y));
            if (el == null) { _highlight.HideRect(); return; }

            var r = UiAutomationUtil.GetBoundingRect(el);
            if (r == null) { _highlight.HideRect(); return; }

            _highlight.ShowRect(WindowCapture.InflateSafe(r.Value, 2));
        }
        catch
        {
            _highlight.HideRect();
        }
    }

    IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _recording && _script != null)
        {
            int msg = wParam.ToInt32();
            if (msg == Win32Hooks.WM_LBUTTONUP)
            {
                try
                {
                    var hs = Marshal.PtrToStructure<Win32Hooks.MSLLHOOKSTRUCT>(lParam);
                    var x = hs.pt.x;
                    var y = hs.pt.y;

                    // Avoid recording clicks on our own overlays by hit-testing UIA at point:
                    var el = AutomationElement.FromPoint(new System.Windows.Point(x, y));
                    if (el != null)
                    {
                        // If caller provided a window filter, try to respect it (best-effort)
                        if (IsProbablyOurOverlay(el))
                            return Win32Hooks.CallNextHookEx(_mouseHook, nCode, wParam, lParam);

                        RecordClick(el);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        return Win32Hooks.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    bool IsProbablyOurOverlay(AutomationElement el)
    {
        // Best-effort: our overlays are tool windows and usually have our process id
        try
        {
            var pid = el.Current.ProcessId;
            if (pid == Environment.ProcessId)
            {
                var name = el.Current.Name ?? "";
                var cls = el.Current.ClassName ?? "";
                // overlays may have empty name/class; keep minimal
                if (name.Contains("REC", StringComparison.OrdinalIgnoreCase)) return true;
                if (cls.Contains("WindowsForms", StringComparison.OrdinalIgnoreCase)) return false; // most apps are WinForms too
            }
        }
        catch { }
        return false;
    }

    void RecordClick(AutomationElement el)
    {
        if (_script == null) return;

        var target = UiAutomationUtil.BuildTargetFromElement(el);

        // Prefer stable selectors: AutomationId; otherwise Name+ControlType; otherwise ClassName+Name
        if (target.AutomationId == null && target.Name == null && target.ClassName == null)
        {
            // If nothing useful, keep ControlType at least
            target.ControlType ??= "Control";
        }

        var step = new Step
        {
            Action = ActionKind.Click,
            Target = target
        };

        // Optional screenshot capture
        if (CaptureScreenshots && !string.IsNullOrWhiteSpace(ScriptPathForAssets))
        {
            try
            {
                var rect = UiAutomationUtil.GetBoundingRect(el);
                if (rect != null)
                {
                    var r = WindowCapture.InflateSafe(rect.Value, 6);
                    var bytes = WindowCapture.CaptureScreenRect(r);

                    var assetsDir = Assets.EnsureAssetsDir(ScriptPathForAssets!);
                    var path = Assets.NewPngName(assetsDir, "click");

                    File.WriteAllBytes(path, bytes);

                    // Store relative-ish hint in Value (keeps model simple)
                    step.Value = Path.GetFileName(path);
                }
            }
            catch
            {
                // ignore screenshot failures
            }
        }

        _script.Steps.Add(step);
        _recOverlay.SetText($"● REC (steps: {_script.Steps.Count})");
    }
}
