using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
namespace WatchDog
{

    public class Program
    {
        private static readonly string WatchListPath = Path.Combine(Environment.CurrentDirectory, "watched_files.json");

        public static void Main(string[] args)
        {
            AnsiConsole.Clear();
            var header = @"
        __      _                              __        __          _            _       ____                  
        \.'---.//|                             \ \      / /   __ _  | |_    ___  | |__   |  _ \    ___     __ _ 
         |\./|  \/                              \ \ /\ / /   / _` | | __|  / __| | '_ \  | | | |  / _ \   / _` |
        _|.|.|_  \                               \ V  V /   | (_| | | |_  | (__  | | | | | |_| | | (_) | | (_| |
       /(  ) ' '  \                               \_/\_/     \__,_|  \__|  \___| |_| |_| |____/   \___/   \__, |
      |  \/   . |  \                                                                                       |___/ 
       \_/\__/| |                                                                                                 
        V  /V / |                                                 A simple file watcher for .NET 6+                                               
          /__/ /                                                                                                 
          \___/                                                                                                  
                                                                                                               
                        
   ";
            // Render the header block in green without altering whitespace
            AnsiConsole.Write(
                new Text(header, new Style(foreground: Color.Green))
            );
     
            var cliArgs = new Queue<string>(args);
            while (true)
            {
                AnsiConsole.Write(new Rule());
                var command = cliArgs.Count > 0
                    ? cliArgs.Dequeue().ToLowerInvariant()
                    : PromptSelectCommand();

                switch (command)
                {
                    case "add":
                        var addPaths = cliArgs.Count > 0
                            ? new List<string> { cliArgs.Dequeue() }
                            : PromptSelectFiles();
                        foreach (var addPath in addPaths)
                        {
                            if (!string.IsNullOrEmpty(addPath))
                                AddFile(addPath);
                        }
                        break;

                    case "remove":
                        var removePaths = cliArgs.Count > 0
                            ? new List<string> { cliArgs.Dequeue() }
                            : PromptSelectWatchedFiles();
                        foreach (var removePath in removePaths)
                        {
                            if (!string.IsNullOrEmpty(removePath))
                                RemoveFile(removePath);
                        }
                        break;

                    case "list":
                        ListEntries();
                        break;

                    case "check":
                        CheckEntries();
                        break;

                    case "exit":
                        AnsiConsole.MarkupLine("[green]Goodbye![/]");
                        return;

                    default:
                        AnsiConsole.MarkupLine($"[red]Unknown command:[/] {command}");
                        break;
                }
            }
        }

        public static Dictionary<string, string> GetWatchedEntries()
        {
            if (!File.Exists(WatchListPath))
                return new Dictionary<string, string>();

            try
            {
                var json = File.ReadAllText(WatchListPath);
                var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return entries ?? new Dictionary<string, string>();
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Corrupted watch list file. Starting fresh.");
                return new Dictionary<string, string>();
            }
        }

        public static void WriteWatchedEntries(Dictionary<string, string> entries)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(entries, options);
            File.WriteAllText(WatchListPath, json);
        }


        public static List<string> PromptSelectFiles()
        {
            var currentDir = Path.GetPathRoot(Environment.CurrentDirectory) 
                ?? Environment.CurrentDirectory;
            while (true)
            {
                // Handle Backspace for going up a directory
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        var parent = Directory.GetParent(currentDir)?.FullName;
                        if (!string.IsNullOrEmpty(parent))
                        {
                            currentDir = parent;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[yellow]Already at root directory.[/]");
                        }
                        continue;
                    }
                }

                List<string> dirs;
                try
                {
                    dirs = Directory.GetDirectories(currentDir)
                        .Select(path => Path.GetFileName(path) ?? string.Empty)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();
                }
                catch (UnauthorizedAccessException)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning:[/] Access denied to {currentDir}. Moving up.");
                    currentDir = Directory.GetParent(currentDir)?.FullName
                        ?? currentDir;
                    continue;
                }
                var choices = new List<string>();
                if (Directory.GetParent(currentDir) != null)
                    choices.Add("..");
                choices.AddRange(dirs);
                choices.Add("[Select files in this directory]");

                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Current directory: {currentDir}")
                        .PageSize(10)
                        .UseConverter(item => Markup.Escape(item))
                        .AddChoices(choices)
                );

                if (selection == "..")
                {
                    var parent = Directory.GetParent(currentDir)?.FullName;
                    if (!string.IsNullOrEmpty(parent))
                    {
                        currentDir = parent;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Already at root directory.[/]");
                    }
                }
                else if (selection == "[Select files in this directory]")
                {
                    List<string> files;
                    try
                    {
                        files = Directory.GetFiles(currentDir).ToList();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Warning:[/] Access denied to files in {currentDir}. Moving up.");
                        currentDir = Directory.GetParent(currentDir)?.FullName
                            ?? currentDir;
                        continue;
                    }
                    if (files.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]No files here.[/]");
                        continue;
                    }
                    return AnsiConsole.Prompt(
                        new MultiSelectionPrompt<string>()
                            .Title("Select files to watch:")
                            .PageSize(10)
                            .UseConverter(item => Markup.Escape(item))
                            .AddChoices(files)
                    );
                }
                else
                {
                    currentDir = Path.Combine(currentDir, selection);
                }
            }
        }

        public static List<string> PromptSelectWatchedFiles()
        {
            var entries = GetWatchedEntries();
            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No files to remove.[/]");
                return new List<string>();
            }
            return AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select files to remove:")
                    .PageSize(10)
                    .UseConverter(item => Markup.Escape(item))
                    .AddChoices(entries.Keys)
            );
        }

        public static string PromptSelectCommand()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a command:")
                    .PageSize(4)
                    .UseConverter(item => Markup.Escape(item))
                    .AddChoices("add", "remove", "list", "check", "exit")
            );
        }

        public static void AddFile(string filePath)
        {
            var entries = GetWatchedEntries();
            var absolutePath = Path.GetFullPath(filePath);
            if (!File.Exists(absolutePath))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] File not found: {absolutePath}");
                return;
            }

            var hash = HashFile(absolutePath);
            entries[absolutePath] = hash;
            WriteWatchedEntries(entries);
            AnsiConsole.MarkupLine($"[green]Added:[/] {absolutePath}");
        }

        public static void RemoveFile(string filePath)
        {
            var entries = GetWatchedEntries();
            var absolutePath = Path.GetFullPath(filePath);
            if (entries.Remove(absolutePath))
            {
                WriteWatchedEntries(entries);
                AnsiConsole.MarkupLine($"[green]Removed:[/] {absolutePath}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Not in watch list: {absolutePath}");
            }
        }

        public static void ListEntries()
        {
            var entries = GetWatchedEntries();
            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No files are currently being watched.[/]");
                return;
            }
            foreach (var kvp in entries)
                AnsiConsole.MarkupLine($"[green]File:[/] {kvp.Key} [blue]Hash:[/] {kvp.Value}");
        }

        public static void CheckEntries()
        {
            var entries = GetWatchedEntries();
            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No files to check.[/]");
                return;
            }
            foreach (var kvp in entries)
            {
                var currentHash = HashFile(kvp.Key);
                if (currentHash == kvp.Value)
                    AnsiConsole.MarkupLine($"[green]OK:[/] {kvp.Key}");
                else
                    AnsiConsole.MarkupLine($"[red]CHANGED:[/] {kvp.Key}");
            }
        }

        public static string HashFile(string filePath)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
