using System.CommandLine;

var pathArg = new Argument<DirectoryInfo>(
    name: "path",
    description: "The root directory to clean. Defaults to the current directory.",
    getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()));

var dryRunOption = new Option<bool>(
    aliases: ["--dry-run", "-d"],
    description: "List folders that would be deleted without actually deleting them.");

var verboseOption = new Option<bool>(
    aliases: ["--verbose", "-v"],
    description: "Show each folder as it is deleted.");

var rootCommand = new RootCommand("Removes all bin and obj folders from a .NET solution or project tree.")
{
    pathArg,
    dryRunOption,
    verboseOption
};

rootCommand.SetHandler((DirectoryInfo path, bool dryRun, bool verbose) =>
{
    if (!path.Exists)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Directory not found: {path.FullName}");
        Console.ResetColor();
        Environment.Exit(1);
    }

    var targets = path
        .EnumerateDirectories("*", SearchOption.AllDirectories)
        .Where(d => d.Name is "bin" or "obj")
        // Skip dirs that are inside another bin/obj (already handled by parent deletion)
        .Where(d => !IsBeneathBinOrObj(d))
        // Only delete bin/obj folders that live inside a .NET project directory
        .Where(d => IsProjectDirectory(d.Parent))
        .OrderByDescending(d => d.FullName.Length) // deepest first
        .ToList();

    if (targets.Count == 0)
    {
        Console.WriteLine("Nothing to clean — no bin or obj folders found.");
        return;
    }

    if (dryRun)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Dry run — {targets.Count} folder(s) would be deleted:\n");
        Console.ResetColor();
        foreach (var dir in targets)
            Console.WriteLine($"  {dir.FullName}");
        return;
    }

    int deleted = 0, failed = 0;

    foreach (var dir in targets)
    {
        try
        {
            if (verbose)
                Console.WriteLine($"  Deleting {dir.FullName}");

            dir.Delete(recursive: true);
            deleted++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"  Failed to delete {dir.FullName}: {ex.Message}");
            Console.ResetColor();
            failed++;
        }
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Super clean complete. {deleted} folder(s) removed.");
    Console.ResetColor();

    if (failed > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{failed} folder(s) could not be deleted.");
        Console.ResetColor();
    }
}, pathArg, dryRunOption, verboseOption);

return await rootCommand.InvokeAsync(args);

static bool IsBeneathBinOrObj(DirectoryInfo dir)
{
    var parent = dir.Parent;
    while (parent is not null)
    {
        if (parent.Name is "bin" or "obj")
            return true;
        parent = parent.Parent;
    }
    return false;
}

static bool IsProjectDirectory(DirectoryInfo? dir)
{
    if (dir is null || !dir.Exists)
        return false;

    return dir.EnumerateFiles("*.*proj").Any();
}
