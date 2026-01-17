namespace PortableWinFormsRecorder;

// Placeholder region picker overlay.
public sealed class RegionPickerOverlay : Form
{
    public Rectangle? SelectedRegion { get; private set; }

    public RegionPickerOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        Opacity = 0.15;
        BackColor = Color.Black;
        WindowState = FormWindowState.Maximized;
    }
}
