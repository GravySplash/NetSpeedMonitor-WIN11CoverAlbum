using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;
using System.Windows;
using System.Windows.Input;

// WinForms alias (for tray icon + context menu)
using Forms = System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace NetSpeedMonitor
{
    /// <summary>
    /// Main overlay window.
    ///
    /// Responsibilities:
    /// - Anchor the small transparent window near the taskbar.
    /// - Sample network Rx/Tx bytes every second and display throughput.
    /// - Provide a tray icon for Show/Hide, Startup toggle, and Exit.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Timer _timer;
        private Forms.NotifyIcon? _tray;
        private long _prevRx;
        private long _prevTx;
        private DateTime _prevTime;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += Sample;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AnchorToTaskbar();
            InitTray();
            PrimeCounters();
            _timer.Start();
        }

        /// <summary>
        /// Positions the overlay near the taskbar by using the WorkArea.
        /// Handles taskbar on top/left/right by comparing WorkArea to full screen bounds.
        /// </summary>
        private void AnchorToTaskbar()
        {
            var screen = Forms.Screen.PrimaryScreen!;
            var wa = SystemParameters.WorkArea;
            var sb = screen.Bounds;

            double x = wa.Right - Width - 10;
            double y = wa.Bottom - Height - 10;

            if (wa.Top > sb.Top)   y = wa.Top + 10;                // taskbar top
            if (wa.Left > sb.Left) x = wa.Left + 10;               // taskbar left
            if (wa.Right < sb.Right) x = wa.Right - Width - 10;    // taskbar right

            Left = x; Top = y;
        }

        /// <summary>
        /// Creates the WinForms tray icon and context menu.
        /// </summary>
        private void InitTray()
        {
            _tray = new Forms.NotifyIcon
            {
                Text = "NetSpeedMonitor",
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information
            };

            var ctx = new Forms.ContextMenuStrip();
            ctx.Items.Add("Show / Hide", null, (_, __) =>
            {
                if (Visibility == Visibility.Visible) HideWindow();
                else ShowWindow();
            });
            ctx.Items.Add("Start with Windows (Toggle)", null, (_, __) => ToggleStartup());
            ctx.Items.Add(new Forms.ToolStripSeparator());
            ctx.Items.Add("Exit", null, (_, __) => ExitApp());

            _tray.ContextMenuStrip = ctx;
            _tray.DoubleClick += (_, __) =>
            {
                if (Visibility == Visibility.Visible) HideWindow();
                else ShowWindow();
            };
        }

        private void ShowWindow()
        {
            Dispatcher.Invoke(() =>
            {
                Show();
                Activate();
                Topmost = true;
            });
        }

        private void HideWindow()
        {
            Dispatcher.Invoke(Hide);
        }

        /// <summary>
        /// Stops sampling and exits the application.
        /// </summary>
        private void ExitApp()
        {
            _timer.Stop();
            if (_tray is not null)
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
            System.Windows.Application.Current.Shutdown();   // <-- fully qualified, no ambiguity
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void PrimeCounters()
        {
            var (rx, tx) = ReadTotals();
            _prevRx = rx;
            _prevTx = tx;
            _prevTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Timer callback: samples totals, computes per-second rates, and updates the UI.
        /// Runs on a timer thread; UI updates are dispatched to the WPF thread.
        /// </summary>
        private void Sample(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.UtcNow;
                var (rx, tx) = ReadTotals();

                var dt = (now - _prevTime).TotalSeconds;
                if (dt <= 0) return;

                var dRx = rx - _prevRx;
                var dTx = tx - _prevTx;

                var downPerSec = dRx / dt;
                var upPerSec   = dTx / dt;

                _prevRx = rx; _prevTx = tx; _prevTime = now;

                var upText = "↑ " + FormatRate(upPerSec);
                var dnText = "↓ " + FormatRate(downPerSec);

                Dispatcher.Invoke(() =>
                {
                    TxtUp.Text = upText;
                    TxtDown.Text = dnText;

                    if (_tray is not null)
                        _tray.Text = $"Up: {FormatRate(upPerSec)}\nDown: {FormatRate(downPerSec)}";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex); // benign: sampling can fail on transient NICs
            }
        }

        /// <summary>
        /// Reads total bytes received/sent across eligible network adapters.
        /// </summary>
        private static (long rx, long tx) ReadTotals()
        {
            long rx = 0, tx = 0;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()
                         .Where(n =>
                             n.OperationalStatus == OperationalStatus.Up &&
                             n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                             n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                             (n.Description?.IndexOf("Virtual", StringComparison.OrdinalIgnoreCase) ?? -1) < 0))
            {
                try
                {
                    var s4 = nic.GetIPv4Statistics();
                    rx += s4.BytesReceived;
                    tx += s4.BytesSent;
                }
                catch
                {
                    // Some adapters may not expose stats; ignore safely.
                }
            }
            return (rx, tx);
        }

        /// <summary>
        /// Formats a bytes/second value into a human-readable string (B/KB/MB/GB per second).
        /// </summary>
        private static string FormatRate(double bytesPerSec)
        {
            const double K = 1024.0, M = K * 1024.0, G = M * 1024.0;
            string unit; double val;
            if (bytesPerSec >= G) { unit = "GB/s"; val = bytesPerSec / G; }
            else if (bytesPerSec >= M) { unit = "MB/s"; val = bytesPerSec / M; }
            else if (bytesPerSec >= K) { unit = "KB/s"; val = bytesPerSec / K; }
            else { unit = "B/s"; val = bytesPerSec; }
            return val >= 100 ? $"{val:0} {unit}" : $"{val:0.0} {unit}";
        }

        /// <summary>
        /// Toggles “Start with Windows” by adding/removing a HKCU Run key.
        /// </summary>
        private void ToggleStartup()
        {
            try
            {
                const string appName = "NetSpeedMonitor";
                var exe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                using var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);

                if (rk is null) return;

                var existing = rk.GetValue(appName) as string;
                if (string.IsNullOrEmpty(existing))
                {
                    rk.SetValue(appName, $"\"{exe}\"");
                    _tray?.ShowBalloonTip(2000, appName, "Enabled Start with Windows.", Forms.ToolTipIcon.Info);
                }
                else
                {
                    rk.DeleteValue(appName, false);
                    _tray?.ShowBalloonTip(2000, appName, "Disabled Start with Windows.", Forms.ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                _tray?.ShowBalloonTip(2000, "NetSpeedMonitor", "Startup toggle failed: " + ex.Message, Forms.ToolTipIcon.Error);
            }
        }
    }
}
