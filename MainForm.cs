using System.Text.Json;

namespace PortableWinFormsRecorder;

public sealed class MainForm : Form
{
    readonly Button _btnRecord = new() { Text = "Record", Width = 100 };
    readonly Button _btnStop = new() { Text = "Stop", Width = 100, Enabled = false };
    readonly Button _btnRun = new() { Text = "Run", Width = 100 };
    readonly Button _btnInspector = new() { Text = "Inspector", Width = 100 };

    readonly CheckBox _chkScreens = new() { Text = "Capture screenshots", Checked = true, AutoSize = true };

    readonly TextBox _txtScript = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Both,
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10)
    };

    readonly Recorder _recorder = new();
    readonly InspectorController _inspector = new();

    Script _script = new();
    string? _lastScriptPath;

    public MainForm()
    {
        Text = "PortableWinFormsRecorder";
        Width = 1100;
        Height = 700;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
        top.Controls.AddRange(new Control[] { _btnRecord, _btnStop, _btnRun, _btnInspector, _chkScreens });

        Controls.Add(_txtScript);
        Controls.Add(top);

        _btnRecord.Click += (_, __) => StartRecording();
        _btnStop.Click += (_, __) => StopRecording();
        _btnRun.Click += (_, __) => RunScript();
        _btnInspector.Click += (_, __) => ToggleInspector();

        Load += (_, __) => RefreshEditor();
        FormClosing += (_, __) => _inspector.Stop();
    }

    void ToggleInspector()
    {
        if (_inspector.IsRunning)
        {
            _inspector.Stop();
            _btnInspector.Text = "Inspector";
        }
        else
        {
            _inspector.Start();
            _btnInspector.Text = "Inspector (ON)";
        }
    }

    void StartRecording()
    {
        _btnRecord.Enabled = false;
        _btnStop.Enabled = true;

        _script = new Script();

        // Ask where to save now, so we know where to put screenshot assets
        using var sfd = new SaveFileDialog
        {
            Filter = "JSON Script (*.json)|*.json|All files (*.*)|*.*",
            FileName = "script.json"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
        {
            _btnStop.Enabled = false;
            _btnRecord.Enabled = true;
            return;
        }

        _lastScriptPath = sfd.FileName;

        _recorder.CaptureScreenshots = _chkScreens.Checked;
        _recorder.ScriptPathForAssets = _lastScriptPath;

        _recorder.Start(_script);
        RefreshEditor();
    }

    void StopRecording()
    {
        _btnStop.Enabled = false;
        _btnRecord.Enabled = true;

        _recorder.Stop();
        RefreshEditor();

        if (!string.IsNullOrWhiteSpace(_lastScriptPath))
            ScriptIO.Save(_lastScriptPath!, _script);
    }

    void RunScript()
    {
        try
        {
            var json = _txtScript.Text;
            var parsed = JsonSerializer.Deserialize<Script>(json, JsonUtil.Options);
            var script = parsed ?? _script;

            var runner = new Runner();
            runner.Execute(script, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            MessageBox.Show(this, "Run completed.", "Runner", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void RefreshEditor()
    {
        _txtScript.Text = JsonSerializer.Serialize(_script, JsonUtil.Options);
    }
}
