namespace GodotInstaller;

public static class Logger
{
    static ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    public static void LogWarning(string message) => Log(message, ConsoleColor.Yellow);
    public static void LogError(string message) => Log(message, ConsoleColor.Red);

    public static void Log(string message, ConsoleColor color = ConsoleColor.Green, bool resetWhenDone = true) =>
        Write(message, color, true, resetWhenDone);

    public static void LogInline(string message, ConsoleColor color = ConsoleColor.Green, bool resetWhenDone = true) =>
        Write(message, color, false, resetWhenDone);

    static void Write(string message, ConsoleColor color = ConsoleColor.Green, bool newLine = true, bool resetWhenDone = true)
    {
        Console.ForegroundColor = color;
        Console.BackgroundColor = BackgroundColor;

        Console.Write($"{message}{(newLine ? "\n" : "")}");

        if (resetWhenDone)
            Console.ResetColor();
    }
}
