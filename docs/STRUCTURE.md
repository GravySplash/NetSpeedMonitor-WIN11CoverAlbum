# Project Structure

This repo intentionally keeps everything flat and easy to scan.

## Current top-level files

- `NetSpeedMonitor.sln` — Visual Studio solution
- `NetSpeedMonitor.csproj` — project file (WPF + Windows targeting)
- `App.xaml` / `App.xaml.cs` — WPF application entry
- `MainWindow.xaml` / `MainWindow.xaml.cs` — overlay UI + logic
- `AssemblyInfo.cs` — WPF theme resource metadata

## Folders

- `docs/` — documentation
- `bin/`, `obj/`, `publish/` — build artifacts (ignored by git)

## Suggested future structure (optional)

If the project grows, a simple refactor that keeps things discoverable:

- `Services/`
  - `NetworkSampler.cs` (read NIC totals, compute throughput)
  - `StartupRegistry.cs` (toggle startup)
- `Tray/`
  - `TrayIconController.cs`
- `ViewModels/`
  - `MainViewModel.cs` (if adopting MVVM)
- `Views/`
  - `MainWindow.xaml` (and code-behind minimized)

This is not required for the current codebase, but it’s a common way to keep responsibilities separated as features are added.
