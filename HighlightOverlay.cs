namespace PortableWinFormsRecorder;

// Placeholder overlay window.
public sealed class HighlightOverlay : Form
{
    public HighlightOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        Opacity = 0.25;
    }
}
