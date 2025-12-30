# Development Guide

## Build prerequisites

- Windows 10/11
- .NET SDK 8
- Optional: Visual Studio 2022

## Running locally

- Visual Studio: open solution and press F5
- CLI:

```powershell
dotnet restore
dotnet run
```

## Debugging tips

- Sampling runs on a timer thread. UI updates must go through the WPF Dispatcher.
- If throughput appears stuck at 0:
  - Ensure an adapter is `OperationalStatus.Up`
  - Check whether your active adapter is filtered out (e.g., VPN/virtual adapter)

## Where to add changes

- Overlay UI: `MainWindow.xaml`
- Sampling logic and formatting: `MainWindow.xaml.cs`
- Tray menu items: `InitTray()` in `MainWindow.xaml.cs`
- Startup toggle: `ToggleStartup()` in `MainWindow.xaml.cs`

## Recommended conventions

- Prefer small, single-purpose methods.
- When adding new features, consider extracting helper classes instead of growing `MainWindow`.
- Use nullable reference types correctly (`<Nullable>enable</Nullable>` is on).

## Adding a new feature (suggested workflow)

1. Create a small branch.
2. Add/adjust docs if the feature changes behavior or adds UI.
3. Keep PRs focused (one feature per PR is ideal).

## Release/publishing

Example single-file publish:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Automated releases (GitHub Actions)

This repo includes a workflow that builds a `win-x64` single-file release and uploads a `.zip` to GitHub Releases.

1. Create a tag (example):

```powershell
git tag v1.0.0
git push origin v1.0.0
```

2. Wait for the workflow to finish (Actions tab).
3. Download the `.zip` from the GitHub Release created for that tag.

If you want to distribute via GitHub Releases, add:
- versioning policy
- changelog
- signed binaries (optional)
