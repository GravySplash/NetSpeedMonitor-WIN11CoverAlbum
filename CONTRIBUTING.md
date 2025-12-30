# Contributing

Thanks for wanting to improve NetSpeedMonitor.

## Quick start

1. Fork the repo
2. Create a feature branch
3. Make changes (keep them focused)
4. Update docs if behavior changes
5. Open a pull request

## Development setup

- Install .NET SDK 8
- Open `NetSpeedMonitor.sln` in Visual Studio 2022 (recommended)

## Code style

- Keep methods small and readable.
- Avoid introducing heavy frameworks unless there’s a clear payoff.
- Prefer extracting new logic into new files/classes instead of growing `MainWindow.xaml.cs`.
- Keep UI changes in XAML when possible.

See `docs/CODE_STYLE.md` for the lightweight style guide used in this repo.

## What to document in PRs

- What problem it solves
- How to test it manually (steps)
- Screenshots/GIFs for UI changes

## Common improvement ideas

- Per-adapter selection
- Settings persistence (window position, update interval)
- Better adapter filtering (VPNs, vEthernet, etc.)
- Theming and accessibility
