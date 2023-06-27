using System.IO.Compression;
using GodotInstaller.Enums;
using ShellLink;

namespace GodotInstaller.Utils;

class GodotInstaller
{
    public GodotInstallationType GodotInstallationType { get; set; }

    readonly string _downloadsFolderPath = 
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Downloads");

    public GodotInstaller(GodotInstallationType godotInstallationType)
    {
        GodotInstallationType = godotInstallationType;
    }

    /// <summary>
    /// Gets the Godot ZIP file for Windows.
    /// </summary>
    /// <param name="installationType">The type of Godot installation</param>
    /// <returns>The path to the ZIP file as a Task.</returns>
    public async Task<string> GetZipFileAsync()
    {
        var path = _downloadsFolderPath;
        string url = GetGodotUrl(GodotInstallationType);
        var finalPath = Path.Join(path, url.Split("/")[^1]);

        Logger.Log("Downloading...", ConsoleColor.Cyan);

        bool errorOccurred = false;

        using var httpClient = new HttpClient();
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = File.Create(finalPath);

            await contentStream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error downloading file: {ex.Message}");
            errorOccurred = true;
        }

        if (!errorOccurred)
            Logger.Log("Downloaded!");

        return finalPath;
    }

    public void UnzipGodot(string zipPath, string destinationPath)
    {
        Logger.Log("Unzipping...", ConsoleColor.Cyan);

        string unzipDestination = Path.Join(Path.GetTempPath(), "Godot");
        ZipFile.ExtractToDirectory(zipPath, unzipDestination);

        FinishGodotInstallation(unzipDestination, destinationPath);

        Directory.Delete(Path.Join(unzipDestination, GetGodotUrl(GodotInstallationType).Split("/")[^1].Replace(".zip", "")));
        Directory.Delete(unzipDestination);
        File.Delete(Path.Join(_downloadsFolderPath, GetGodotUrl(GodotInstallationType).Split("/")[^1]));

        Logger.Log("Unzipped!", ConsoleColor.Green);
    }

    private void FinishGodotInstallation(string unzipDestination, string destinationPath)
    {
        string url = GetGodotUrl(GodotInstallationType);
        string directoryPath = Path.Join(unzipDestination, 
            url.Split("/")[^1].Replace(".zip", ""));
        string[] files = Directory.GetFiles(directoryPath);
        string[] directories = Directory.GetDirectories(directoryPath);

        Directory.CreateDirectory(destinationPath);

        foreach (string file in files)
            MoveItem(file, destinationPath, false);

        foreach (string directory in directories)
            MoveItem(directory, destinationPath, true);
    }

    private static void MoveItem(string item, string destinationPath, bool isDirectory)
    {
        string destinationFilePath = Path.Combine(destinationPath, 
            Path.GetFileName(item));
        
        if (isDirectory)
            Directory.Move(item, destinationFilePath);
        else
            File.Move(item, destinationFilePath);
    }

    public Shortcut CreateGodotDesktopShortcut(string exePath)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var shortcutPath = Path.Join(desktopPath, "Godot Engine.lnk");

        if (Path.Exists(shortcutPath)) 
            throw new IOException("Shortcut file already exists.");

        Logger.Log("Creating desktop shortcut...", ConsoleColor.Cyan);
        Shortcut shortcut = Shortcut.CreateShortcut(exePath);

        Logger.Log("Writing...", ConsoleColor.Cyan);
        shortcut.WriteToFile(shortcutPath);

        Logger.Log("Desktop Shortcut Created!", ConsoleColor.Green);
        return shortcut;
    }

    public string GetGodotUrl(GodotInstallationType installationType)
    {
        var gdPath = "https://github.com/godotengine/godot/releases/download/4.0.3-stable/Godot_v4.0.3-stable_win64.exe.zip";
        var csPath = "https://github.com/godotengine/godot/releases/download/4.0.3-stable/Godot_v4.0.3-stable_mono_win64.zip";
        return installationType == GodotInstallationType.GDScript ? gdPath : csPath;
    }
}
