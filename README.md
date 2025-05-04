# WatchDog

WatchDog is a simple command‑line file watcher built on .NET 6+. It lets you register files (by hashing them), and later verify whether any registered file has been modified.

---

## Features

- **Add files** to the watch list (single or multiple at once) via interactive directory traversal.
- **Remove files** from the watch list with multi‑selection.
- **List** all watched files along with their stored hashes.
- **Check** watched files to report “OK” or “CHANGED” status.
- **Interactive menu**—run without arguments to pick commands in a loop.
- **Exit** command to quit the program cleanly.
- **JSON storage** (`watched_files.json`) for easy manual inspection or editing.
- **Unauthorized‑access handling**: skips protected directories and moves up automatically.
- **Backspace navigation**: go up one directory while browsing.

---

## Download

Official binaries are available on the [Releases page](https://github.com/Jimmy-Tollett/WatchDog/releases/latest):
- [Windows](https://github.com/Jimmy-Tollett/WatchDog/releases/download/v1.0.0/WatchDog-win-x64.zip)
- [Linux](https://github.com/Jimmy-Tollett/WatchDog/releases/download/v1.0.0/WatchDog-linux-x64.zip)
- [MacOS (Intel Based)](https://github.com/Jimmy-Tollett/WatchDog/releases/download/v1.0.0/WatchDog-osx-x64.zip)
- [MacOS (Apple Silicon)](https://github.com/Jimmy-Tollett/WatchDog/releases/download/v1.0.0/WatchDog-osx-arm64.zip)

---

## Usage

You can invoke WatchDog with or without arguments.

### Non‑interactive mode

```bash
dotnet run -- add /path/to/file.txt
dotnet run -- remove /path/to/file.txt
dotnet run -- list
dotnet run -- check
dotnet run -- exit
```

### Interactive menu

Run without arguments:

```bash
dotnet run
```

You’ll be presented with a menu:

1. **add**  
   - Navigate starting at your system’s root.
   - Use arrow keys to enter directories, or Backspace to go up.
   - Choose `[Select files in this directory]` to pick one or more files.

2. **remove**  
   - Multi‑select from the current watch list.

3. **list**  
   - Show each file path and its stored SHA‑256 hash.

4. **check**  
   - Recompute hashes and report any changes.

5. **exit**  
   - Quit the program.

---

## Configuration & Storage

- All watched entries are stored in `watched_files.json` in the current working directory.
- Format:  
  ```json
  {
    "/full/path/to/file1.txt": "abc123…",
    "/full/path/to/another.file": "def456…"
  }
  ```

---

## Extending WatchDog

- **Automatic scheduling**: integrate with cron (Linux/macOS) or Task Scheduler (Windows).
- **Notifications**: hook into email or desktop alerts on change.
- **Recursive directories**: add whole folder watches with directory‑tree hashing.
- **Custom hash algorithms**: switch from SHA‑256 to MD5, SHA‑1, or others.

---

## Build from Source

1. **Clone the repository**  
   ```bash
   git clone https://github.com/your‑org/WatchDog.git
   cd WatchDog
   ```

2. **Build**  
   ```bash
   dotnet build
   ```

3. **Run**  
   ```bash
   dotnet run -- [command] [filePath]
   ```

---

## Contributing

1. Fork the repo  
2. Create a feature branch (`git checkout -b feature/YourFeature`)  
3. Commit your changes (`git commit -m "Add YourFeature"`)  
4. Push to your branch (`git push origin feature/YourFeature`)  
5. Open a Pull Request

