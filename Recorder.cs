using System;
using System.IO;
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

    readonly System.Windows.Forms.Timer _hoverTimer = new() { Interval = 50 };

    public bool CaptureScreenshots { get; set; } = true;

    // Script path used to locate/create script_assets folder
    public string? ScriptPathForAssets { get; set; }

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

        var p = System.Windows.Forms.Cursor.Position;
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

                    var el = AutomationElement.FromPoint(new System.Windows.Point(x, y));
                    if (el != null)
                        RecordClick(el);
                }
                catch
                {
                    // ignore
                }
            }
        }

        return Win32Hooks.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    void RecordClick(AutomationElement el)
    {
        if (_script == null) return;

        var target = UiAutomationUtil.BuildTargetFromElement(el);

        var step = new Step
        {
            Action = ActionKind.Click,
            Target = target
        };

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
                    var filePath = Assets.NewPngName(assetsDir, "click");

                    System.IO.File.WriteAllBytes(filePath, bytes);

                    // Store just filename
                    step.Value = System.IO.Path.GetFileName(filePath);
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
