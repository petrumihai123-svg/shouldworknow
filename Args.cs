namespace PortableWinFormsRecorder;

public sealed class Args
{
    public string Command { get; set; } = "";

    public string? Process { get; set; }       // --process MyApp
    public string? ScriptPath { get; set; }    // --script script.json
    public string? OutPath { get; set; }       // --out script.json
    public string? DataPath { get; set; }      // --data data.csv
    public bool CaptureImages { get; set; }    // --capture-images (reserved)
    public bool Help { get; set; }

    public static Args Parse(string[] argv)
    {
        var a = new Args();
        if (argv.Length == 0) { a.Help = true; return a; }

        a.Command = argv[0].Trim().ToLowerInvariant();
        for (int i = 1; i < argv.Length; i++)
        {
            var t = argv[i];

            if (t is "-h" or "--help") { a.Help = true; continue; }
            if (t == "--capture-images") { a.CaptureImages = true; continue; }

            string? next = (i + 1 < argv.Length) ? argv[i + 1] : null;

            if (t == "--process" && next != null) { a.Process = next; i++; continue; }
            if (t == "--script" && next != null) { a.ScriptPath = next; i++; continue; }
            if (t == "--out" && next != null) { a.OutPath = next; i++; continue; }
            if (t == "--data" && next != null) { a.DataPath = next; i++; continue; }
        }

        return a;
    }
}
