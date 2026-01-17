namespace PortableWinFormsRecorder;

public sealed class HighlightOverlay : Form
{
    Rectangle _rect = Rectangle.Empty;

    public HighlightOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;

        BackColor = Color.LimeGreen;
        TransparencyKey = Color.LimeGreen;

        // Click-through
        SetStyle(ControlStyles.Selectable, false);
        Enabled = false;
        Opacity = 0.75;
    }

    public void ShowRect(Rectangle rect)
    {
        _rect = rect;
        if (_rect.Width < 2) _rect.Width = 2;
        if (_rect.Height < 2) _rect.Height = 2;

        Bounds = _rect;
        if (!Visible) Show();
        Invalidate();
    }

    public void HideRect()
    {
        if (Visible) Hide();
        _rect = Rectangle.Empty;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_rect == Rectangle.Empty) return;

        using var pen = new Pen(Color.Red, 3);
        e.Graphics.DrawRectangle(pen, new Rectangle(1, 1, Width - 3, Height - 3));
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const int WS_EX_NOACTIVATE = 0x08000000;

            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }
}
