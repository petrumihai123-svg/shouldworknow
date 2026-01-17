namespace PortableWinFormsRecorder;

// Reserved for future image-capture assets.
public static class Assets
{
    public static string GetAssetsDirNearScript(string scriptPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(scriptPath)) ?? Environment.CurrentDirectory;
        return Path.Combine(dir, "script_assets");
    }
}
