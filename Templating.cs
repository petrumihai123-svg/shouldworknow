namespace PortableWinFormsRecorder;

public static class Templating
{
    // Replaces {{Key}} with data[Key]. Unknown keys -> empty.
    public static string Apply(string? input, IReadOnlyDictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(input)) return input ?? "";

        var s = input;
        foreach (var kv in data)
            s = s.Replace("{{" + kv.Key + "}}", kv.Value ?? "", StringComparison.OrdinalIgnoreCase);

        // If any templates remain, blank them
        while (true)
        {
            var start = s.IndexOf("{{", StringComparison.Ordinal);
            if (start < 0) break;
            var end = s.IndexOf("}}", start + 2, StringComparison.Ordinal);
            if (end < 0) break;
            s = s.Remove(start, (end + 2) - start);
        }

        return s;
    }
}
