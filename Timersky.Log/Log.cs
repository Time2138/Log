using System.Diagnostics;

namespace Timersky.Log;

public sealed class Log
{
    private static string? _logFilePath;

    /// <summary>
    /// Initializes the logging system by setting up the log file directory and creating a new log file.
    /// </summary>
    /// <param name="logDirPath">The directory where log files should be stored. If not specified, defaults to the "logs" folder in the application's base directory.</param>
    public static void Initialize(string logDirPath = "")
    {
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            return;
        }
        
        if (string.IsNullOrEmpty(logDirPath))
        {
            logDirPath = $"{AppDomain.CurrentDomain.BaseDirectory}logs/";
        }
    
        _logFilePath = $"{logDirPath}{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.log";
    
        if (!Directory.Exists(logDirPath))
        {
            Directory.CreateDirectory(logDirPath);
        }
    
        if (!File.Exists(_logFilePath))
        {
            File.Create(_logFilePath).Close();
        }
    }

    
    /// <summary>
    /// Logs an informational message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    public static void Info(string message) => WriteConsole(message, GetSender(), LogType.Info, DateTime.UtcNow);

    /// <summary>
    /// Logs a warning message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public static void Warning(string message) => WriteConsole(message, GetSender(), LogType.Warning, DateTime.UtcNow);

    /// <summary>
    /// Logs an error message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public static void Error(string message) => WriteConsole(message, GetSender(), LogType.Error, DateTime.UtcNow);

    /// <summary>
    /// Logs a debug message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    public static void Debug(string message) => WriteConsole(message, GetSender(), LogType.Debug, DateTime.UtcNow);

    /// <summary>
    /// Logs an informational object message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The informational object to log. Its string representation is used.</param>
    public static void Info(object message) => WriteConsole(message.ToString() ?? string.Empty, GetSender(), LogType.Info, DateTime.UtcNow);

    /// <summary>
    /// Logs a warning object message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The warning object to log. Its string representation is used.</param>
    public static void Warning(object message) => WriteConsole(message.ToString() ?? string.Empty, GetSender(), LogType.Warning, DateTime.UtcNow);

    /// <summary>
    /// Logs an error object message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The error object to log. Its string representation is used.</param>
    public static void Error(object message) => WriteConsole(message.ToString() ?? string.Empty, GetSender(), LogType.Error, DateTime.UtcNow);

    /// <summary>
    /// Logs a debug object message to both the console and a file. The timestamp is included only in the file.
    /// </summary>
    /// <param name="message">The debug object to log. Its string representation is used.</param>
    public static void Debug(object message) => WriteConsole(message.ToString() ?? string.Empty, GetSender(), LogType.Debug, DateTime.UtcNow);
    
    /// <summary>
    /// Reads a line of input from the console and logs it to a file with a timestamp.
    /// </summary>
    /// <returns>The input string entered by the user, or null if no input was provided.</returns>
    public static string? Read(bool secure = false)
    {
        if (string.IsNullOrEmpty(_logFilePath))
        {
            throw new LoggerNotInitializedException();
        }
        
        string? message = Console.ReadLine();

        if (secure)
        {
            WriteFile("*************", string.Empty, LogType.In, DateTime.UtcNow);
        }
        else
        {
            WriteFile(message ?? string.Empty, string.Empty, LogType.In, DateTime.UtcNow);
        }
        
        return message;
    }

    /// <summary>
    /// Writes a log message to the console with formatting based on log type and logs the same message to a file with a timestamp.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="sender">The sender information to associate with the log.</param>
    /// <param name="type">The type of log (e.g., Info, Warning, Error, Debug).</param>
    /// <param name="time">The timestamp for the log entry. This is only written to the file, not displayed in the console.</param>
    public static void WriteConsole(string message, string sender, LogType type, DateTime time)
    {
        if (string.IsNullOrEmpty(_logFilePath))
        {
            throw new LoggerNotInitializedException();
        }
        
        var backColor = Console.BackgroundColor;
        var foreColor = Console.ForegroundColor;

        var logColor = type switch
        {
            LogType.Info => ConsoleColor.Cyan,
            LogType.Warning => ConsoleColor.Yellow,
            LogType.Error => ConsoleColor.Red,
            LogType.Debug => ConsoleColor.Green,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = backColor;
        Console.BackgroundColor = logColor;

        Console.Write($"{_logNames[type]}");
        
        Console.ForegroundColor = logColor;
        Console.BackgroundColor = backColor;
        
        Console.Write(" ");
        
        Console.ForegroundColor = backColor;
        Console.BackgroundColor = logColor;
        
        Console.Write($"{sender}");

        Console.ForegroundColor = logColor;
        Console.BackgroundColor = backColor;
        
        Console.Write(":");
        Console.Write($" {message}");
        Console.Write("\n");
        
        Console.ForegroundColor = foreColor;
        
        WriteFile(message, sender, type, time);
    }
    
    private static void WriteFile(string message, string sender, LogType type, DateTime time)
    {
        if (_logFilePath == null) return;
        
        using (FileStream file = new(_logFilePath, FileMode.Append))
        {
            using (StreamWriter writer = new(file))
            {
                writer.WriteLine(type == LogType.In ? $"|{time:yyyy-MM-dd HH:mm:ss:ffff}| |STDIN | {message}" : $"|{time:yyyy-MM-dd HH:mm:ss:ffff}| |STDOUT| |{_logNames[type]}| |{sender}| {message}");
            }
        }
    }
    
    private static string GetSender()
    {
        StackFrame stackFrame = new(2);
        return $"{stackFrame.GetMethod()!.DeclaringType}.{stackFrame.GetMethod()!.Name}";
    } 
    
    private static readonly Dictionary<LogType, string> _logNames = new()
    {
        { LogType.Info,    "INFORMATION" },
        { LogType.Warning, "  WARNING  " },
        { LogType.Error,   "   ERROR   " },
        { LogType.Debug,   "   DEBUG   " }
    };
}
