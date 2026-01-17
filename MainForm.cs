using System.Text.Json;

namespace PortableWinFormsRecorder;

public sealed class MainForm : Form
{
    readonly Button _btnRecord = new() { Text = "Record", Width = 100 };
    readonly Button _btnStop = new() { Text = "Stop", Width = 100, Enabled = false };
    readonly Button _btnRun = new() { Text = "Run", Width = 100 };
    readonly TextBox _txtScript = new() { Multiline = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };

    readonly Recorder _recorder = new();
    Script _script = new();

    public MainForm()
    {
        Text = "PortableWinFormsRecorder";
        Width = 1100;
        Height = 700;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
        top.Controls.AddRange(new Control[] { _btnRecord, _btnStop, _btnRun });

        Controls.Add(_txtScript);
        Controls.Add(top);

        _btnRecord.Click += (_, __) => StartRecording();
        _btnStop.Click += (_, __) => StopRecording();
        _btnRun.Click += (_, __) => RunScript();

        Load += (_, __) => RefreshEditor();
    }

    void StartRecording()
    {
        _btnRecord.Enabled = false;
        _btnStop.Enabled = true;

        _script = new Script();
        _recorder.Start(_script);
        RefreshEditor();
    }

    void StopRecording()
    {
        _btnStop.Enabled = false;
        _btnRecord.Enabled = true;

        _recorder.Stop();
        RefreshEditor();

        using var sfd = new SaveFileDialog
        {
            Filter = "JSON Script (*.json)|*.json|All files (*.*)|*.*",
            FileName = "script.json"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
            ScriptIO.Save(sfd.FileName, _script);
    }

    void RunScript()
    {
        try
        {
            // If editor has valid JSON, use it
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
