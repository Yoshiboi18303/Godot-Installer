using GodotInstaller.Enums;
using GodotInstaller.Utils;

namespace GodotInstaller;

class Input
{
    public GodotInstallationType GodotInstallationType { get; private set; }
    public string DestinationPath { get; private set; }

    public bool CreateDesktopShortcut { get; private set; }

    public Input(GodotInstallationType godotInstallationType, string destinationPath, bool createDesktopShortcut)
    {
        GodotInstallationType = godotInstallationType;
        DestinationPath = destinationPath;
        CreateDesktopShortcut = createDesktopShortcut;
    }
}

class Program
{
    public static void Main()
    {
        Console.Clear();
        Logger.Log("Godot Installer\n", ConsoleColor.Cyan);
        Logger.Log("WARNING: This is NOT an official installer, and is NOT maintained by the owners of the Godot Engine.", ConsoleColor.Red);
        Logger.LogInline("Press enter to start, press Ctrl+C otherwise.", ConsoleColor.Gray);
        Console.ReadLine();

        var answers = GetAnswers();

        Utils.GodotInstaller godotInstaller = new(answers.GodotInstallationType);

        var path = godotInstaller.GetZipFileAsync().Result;

        godotInstaller.UnzipGodot(path, answers.DestinationPath);

        if (answers.CreateDesktopShortcut)
        {
            godotInstaller.CreateGodotDesktopShortcut(Path.Join(answers.DestinationPath, GetExe(godotInstaller.GetGodotUrl(answers.GodotInstallationType).Split("/")[^1])));
        }

        Logger.LogInline($"Godot Engine{(answers.GodotInstallationType == GodotInstallationType.Mono ? " Mono" : "")} installed at \"{answers.DestinationPath}\"! Press enter to exit.", ConsoleColor.Green);
        Console.ReadLine();
    }

    private static string GetExe(string name)
    {
        string final;

        if (name.Contains(".exe"))
        {
            final = name.Replace(".zip", "");
        }
        else
        {
            final = name.Replace(".zip", ".exe");
        }

        return final;
    }

    private static Input GetAnswers()
    {
        Console.Clear();

        Console.Write("Please enter your Godot type (Normal/Mono): ");
        var godotType = Console.ReadLine();

        if (godotType is not "Normal" and not "Mono")
        {
            Logger.Log("Invalid input provided, assuming Normal...", ConsoleColor.Red);
        }

        GodotInstallationType installationType = godotType == "Mono" ? GodotInstallationType.Mono : GodotInstallationType.GDScript;

        Logger.Log($"\nNOTE: Do NOT enter a path that requires administrative permissions (for example: {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}), doing so can result in unexpected issues.", ConsoleColor.Yellow);
        Logger.Log($@"NOTE: Do NOT enter the Godot AppData path ({Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Godot), doing so may cause unexpected issues while using Godot.", ConsoleColor.Yellow);
        Logger.Log($"TIP: Want to put it on your Desktop? Add \"&DESKTOP\" to the start of the path!\n", ConsoleColor.Cyan);

        Console.Write("Please enter your destination path: ");
        var destinationPath = Console.ReadLine();
        destinationPath = destinationPath.Replace("&DESKTOP", $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
        bool pathValid = false;

        while (!pathValid)
        {
            if (IsValidPath(destinationPath))
            {
                pathValid = true;
            }
            else
            {
                Logger.Log("ERROR: Invalid path, please try again.", ConsoleColor.Red);
                Console.Write("Please enter your destination path: ");
                destinationPath = Console.ReadLine();
                destinationPath = destinationPath.Replace("&DESKTOP", $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
            }
        }

        Console.Write("Would you like a Desktop shortcut to be created? (Y/n) ");
        var createDesktopShortcutInput = Console.ReadLine().ToLower();
        bool createDesktopShortcut = createDesktopShortcutInput is "" or "y" or "yes";

        return new(installationType, destinationPath, createDesktopShortcut);
    }

    private static bool IsValidPath(string destinationPath)
    {
        bool pathHasInvalidCharacters = false;

        foreach (char character in Path.GetInvalidPathChars())
        {
            if (destinationPath.ToCharArray().Contains(character))
            {
                pathHasInvalidCharacters = true;
                break;
            }
            else
            {
                pathHasInvalidCharacters = false;
            }
        }

        return Path.IsPathRooted(destinationPath) && !destinationPath.EndsWith(@"\") && !Path.Exists(destinationPath) && !pathHasInvalidCharacters;
    }
}
