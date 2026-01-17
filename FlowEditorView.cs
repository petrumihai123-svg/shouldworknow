namespace PortableWinFormsRecorder;

// Placeholder for a richer step/flow editor UI.
public sealed class FlowEditorView : UserControl
{
    public FlowEditorView()
    {
        Dock = DockStyle.Fill;
        Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "FlowEditorView (stub)"
        });
    }
}
