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
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var json = JsonSerializer.Serialize(script, JsonUtil.Options);
        File.WriteAllText(path, json);
    }
}
