namespace PortableWinFormsRecorder;

public static class Assets
{
    public static string GetAssetsDirNearScript(string scriptPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(scriptPath)) ?? Environment.CurrentDirectory;
        return Path.Combine(dir, "script_assets");
    }

    public static string EnsureAssetsDir(string scriptPath)
    {
        var assets = GetAssetsDirNearScript(scriptPath);
        Directory.CreateDirectory(assets);
        return assets;
    }

    public static string NewPngName(string assetsDir, string prefix)
    {
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(assetsDir, $"{prefix}_{stamp}.png");
    }
}
