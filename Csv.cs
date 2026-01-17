namespace PortableWinFormsRecorder;

public static class Csv
{
    // Simple CSV reader:
    // - first row headers
    // - second row values
    // Returns dict header->value (missing -> "")
    public static Dictionary<string, string> ReadKeyValueFirstRow(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("CSV not found", path);

        var lines = File.ReadAllLines(path);
        if (lines.Length == 0) return new(StringComparer.OrdinalIgnoreCase);

        var headers = Split(lines[0]);
        var values = (lines.Length > 1) ? Split(lines[1]) : Array.Empty<string>();

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            var key = headers[i].Trim();
            if (string.IsNullOrEmpty(key)) continue;
            var val = (i < values.Length) ? values[i] : "";
            dict[key] = val;
        }
        return dict;
    }

    static string[] Split(string line)
    {
        // Minimal CSV split (no quoted commas support).
        // If you need full CSV support, swap this for a proper parser later.
        return line.Split(',');
    }
}
