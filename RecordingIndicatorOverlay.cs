namespace PortableWinFormsRecorder;

// Placeholder recording indicator overlay.
public sealed class RecordingIndicatorOverlay : Form
{
    public RecordingIndicatorOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        Width = 180;
        Height = 40;

        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "● REC",
            Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold)
        };
        Controls.Add(lbl);
    }
}
