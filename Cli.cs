using System;
using System.IO;
using System.Text.Json;

namespace PortableWinFormsRecorder;

public static class Cli
{
    public static int Run(string[] argv)
    {
        var args = Args.Parse(argv);

        if (args.Help || string.IsNullOrWhiteSpace(args.Command))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return args.Command switch
            {
                "record" => Record(args),
                "run" => RunScript(args),
                "print" => Print(args),
                _ => Unknown(args.Command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 2;
        }
    }

    static int Unknown(string cmd)
    {
        Console.Error.WriteLine($"Unknown command: {cmd}");
        PrintHelp();
        return 1;
    }

    static void PrintHelp()
    {
        Console.WriteLine(
"""
PortableWinFormsRecorder

GUI:
  dotnet run -c Release

CLI:
  dotnet run -c Release -- record --process MyWinFormsApp --out script.json
  dotnet run -c Release -- run --script script.json --data data.csv
  dotnet run -c Release -- print --script script.json

Notes:
  - record is a minimal stub that creates an initial script skeleton.
  - run uses Windows UIAutomation to locate and act on elements.

Script step examples:
  { "action": "Click", "target": { "automationId": "btnOk" } }
  { "action": "SetText", "target": { "automationId": "txtName" }, "value": "{{Name}}" }
  { "action": "AssertTextContains", "target": { "automationId": "lblStatus" }, "value": "Ready" }
""");
    }

    static int Record(Args args)
    {
        if (string.IsNullOrWhiteSpace(args.OutPath))
            throw new ArgumentException("Missing --out <path>");

        var script = new Script
        {
            Steps = new()
            {
                new() { Action = ActionKind.WaitMs, Ms = 250 }
            }
        };

        var outFull = Path.GetFullPath(args.OutPath);
        var outDir = Path.GetDirectoryName(outFull);
        if (!string.IsNullOrWhiteSpace(outDir))
            Directory.CreateDirectory(outDir);

        var json = JsonSerializer.Serialize(script, JsonUtil.Options);
        File.WriteAllText(args.OutPath, json);

        Console.WriteLine($"Wrote stub script: {args.OutPath}");
        return 0;
    }

    static int Print(Args args)
    {
        if (string.IsNullOrWhiteSpace(args.ScriptPath))
            throw new ArgumentException("Missing --script <path>");

        var script = ScriptIO.Load(args.ScriptPath);
        Console.WriteLine(JsonSerializer.Serialize(script, JsonUtil.Options));
        return 0;
    }

    static int RunScript(Args args)
    {
        if (string.IsNullOrWhiteSpace(args.ScriptPath))
            throw new ArgumentException("Missing --script <path>");

        var script = ScriptIO.Load(args.ScriptPath);
        var data = string.IsNullOrWhiteSpace(args.DataPath)
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : Csv.ReadKeyValueFirstRow(args.DataPath);

        var runner = new Runner();
        runner.Execute(script, data);

        Console.WriteLine("Done.");
        return 0;
    }
}

internal static class JsonUtil
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
