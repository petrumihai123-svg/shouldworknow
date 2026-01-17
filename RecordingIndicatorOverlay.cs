namespace PortableWinFormsRecorder;

public sealed class RecordingIndicatorOverlay : Form
{
    readonly Label _lbl = new();

    public RecordingIndicatorOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;

        Width = 140;
        Height = 40;

        BackColor = Color.Black;
        Opacity = 0.80;

        _lbl.Dock = DockStyle.Fill;
        _lbl.TextAlign = ContentAlignment.MiddleCenter;
        _lbl.ForeColor = Color.White;
        _lbl.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        _lbl.Text = "● REC";
        Controls.Add(_lbl);

        // Place top-right
        var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
        Location = new Point(wa.Right - Width - 10, wa.Top + 10);
    }

    public void SetText(string text) => _lbl.Text = text;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const int WS_EX_NOACTIVATE = 0x08000000;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }
}
