# NetSpeedMonitor

A small WPF overlay for Windows that shows **live upload/download speed** near the taskbar.

This repo is intentionally simple: it’s a single-window WPF app with a tray icon, designed to be easy to fork and extend.

## What it does

- Shows **Up** and **Down** throughput (bytes/sec formatted as B/KB/MB/GB per second)
- Anchors itself near the taskbar (bottom/right by default, adjusts for other taskbar positions)
- Runs in the background with a **system tray icon** (Show/Hide, Start with Windows toggle, Exit)
- Window can be **dragged** with left mouse button

## How it works (high level)

- Every second, a timer samples total Rx/Tx bytes from active network interfaces via `System.Net.NetworkInformation`.
- It computes the delta since the last sample to estimate throughput.
- UI is updated on the WPF Dispatcher thread.

See the architecture notes in:
- `docs/ARCHITECTURE.md`
- `docs/STRUCTURE.md`
- `docs/DEVELOPMENT.md`
- `docs/CODE_STYLE.md`

## Requirements

- Windows 10/11
- .NET SDK 8 (project targets `net8.0-windows10.0.19041.0`)
- Optional: Visual Studio 2022 (recommended) for WPF designer support

## Build & run

### Using Visual Studio

1. Open `NetSpeedMonitor.sln`
2. Build and run (F5)

### Using the CLI

```powershell
# from the repo root

# restore
dotnet restore

# build
dotnet build -c Release

# run
dotnet run
```

## Publish (example)

This produces a self-contained single-file build for x64 Windows:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Output will land under `bin/Release/<tfm>/<rid>/publish/`.

## Repo layout

- `App.xaml` / `App.xaml.cs` — WPF application entry
- `MainWindow.xaml` — UI for the overlay
- `MainWindow.xaml.cs` — sampling loop + tray icon + startup toggle
- `docs/` — architecture and contributor docs

## Extending the app

Common extension points (kept intentionally open-ended):

- Show per-adapter speed (instead of summing all active adapters)
- Add settings (position, font size, update interval)
- Switch to MVVM if you want better testability and structure
- Add localization, theming, or additional metrics

See `docs/DEVELOPMENT.md` for recommended direction.

## Contributing

See `CONTRIBUTING.md`.

## Screenshots

Add screenshots/GIFs here to show:
- Overlay on taskbar
- Tray menu

## License

No license file is included yet. If you intend this to be reused publicly, add a `LICENSE` file (MIT is common for small utilities).
