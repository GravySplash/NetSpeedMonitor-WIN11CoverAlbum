# Architecture

This project is a lightweight Windows desktop utility built with **WPF**.

## Goals

- Minimal moving parts (easy to read, easy to fork)
- Runs unobtrusively (small overlay + tray icon)
- Safe sampling loop (handles transient NIC issues)

## Runtime components

### 1) WPF Application (`App`)

- `App.xaml` defines the entry point using `StartupUri="MainWindow.xaml"`.
- `App.xaml.cs` is intentionally empty; WPF initializes the app.

### 2) Overlay Window (`MainWindow`)

- `MainWindow.xaml` defines a transparent, borderless window with two `TextBlock`s.
- The window is set as:
  - `WindowStyle=None`
  - `AllowsTransparency=True`
  - `ShowInTaskbar=False`
  - `Topmost=True`

### 3) Sampling loop

- `MainWindow` uses `System.Timers.Timer` with a 1000ms interval.
- At each tick:
  1. Sum Rx/Tx totals across selected network interfaces.
  2. Compute throughput from the deltas.
  3. Marshal UI updates onto the WPF Dispatcher.

### 4) System tray integration

- The project uses WinForms (`System.Windows.Forms.NotifyIcon`) to show a tray icon.
- The context menu provides:
  - Show/Hide overlay
  - Toggle “Start with Windows”
  - Exit

### 5) Start with Windows

- Implemented by writing a value under:
  - `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`

## Data flow (simplified)

1. Timer tick → read NIC totals
2. Delta compute → format as string
3. Dispatcher → set `TxtUp.Text` / `TxtDown.Text`
4. Optional → update tray tooltip

## Notes and tradeoffs

- Throughput is an estimate based on sampling period and reported byte counters.
- The app currently sums multiple NICs, excluding loopback/tunnel and adapters with “Virtual” in the description.
- For larger feature sets (settings, theming, per-adapter selection), consider moving logic into separate classes or adopting MVVM.
