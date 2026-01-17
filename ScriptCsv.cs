namespace PortableWinFormsRecorder;

// Placeholder for future CSV-to-script conversion.
// Kept so existing filename compiles.
public static class ScriptCsv
{
    public static Script FromCsv(string path)
        => throw new NotSupportedException("CSV->Script conversion not implemented in this baseline.");

    public static void ToCsv(Script script, string path)
        => throw new NotSupportedException("Script->CSV export not implemented in this baseline.");
}
