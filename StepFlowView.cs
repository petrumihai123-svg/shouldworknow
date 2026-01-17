namespace PortableWinFormsRecorder;

// Placeholder for step visualization UI.
public sealed class StepFlowView : UserControl
{
    public StepFlowView()
    {
        Dock = DockStyle.Fill;
        Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "StepFlowView (stub)"
        });
    }
}
