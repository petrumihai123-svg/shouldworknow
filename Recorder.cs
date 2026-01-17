namespace PortableWinFormsRecorder;

// Minimal stub recorder.
// Extend this by hooking global mouse/keyboard events and building steps.
public sealed class Recorder
{
    Script? _script;
    bool _recording;

    public bool IsRecording => _recording;

    public void Start(Script script)
    {
        _script = script ?? throw new ArgumentNullException(nameof(script));
        _recording = true;

        // Put a placeholder to show structure
        _script.Steps.Add(new Step { Action = ActionKind.WaitMs, Ms = 250 });
        _script.Steps.Add(new Step
        {
            Action = ActionKind.AssertExists,
            Target = new Target { AutomationId = "exampleAutomationId" }
        });
    }

    public void Stop()
    {
        _recording = false;
        _script = null;
    }
}
