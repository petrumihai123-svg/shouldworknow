using System;
using System.IO;
using System.Text.Json;

namespace PortableWinFormsRecorder;

public static class ScriptIO
{
    public static Script Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Script not found", path);

        var json = File.ReadAllText(path);
        var script = JsonSerializer.Deserialize<Script>(json, JsonUtil.Options);
        return script ?? new Script();
    }

    public static void Save(string path, Script script)
    {
        var full = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(script, JsonUtil.Options);
        File.WriteAllText(path, json);
    }
}
