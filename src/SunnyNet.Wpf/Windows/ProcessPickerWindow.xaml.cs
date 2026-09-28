using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SunnyNet.Wpf.Windows;

public partial class ProcessPickerWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExLayered = 0x00080000;
    private const uint GaRoot = 2;
    private const int DwmwaExtendedFrameBounds = 9;

    public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
        nameof(HintText),
        typeof(string),
        typeof(ProcessPickerWindow),
        new PropertyMetadata("将准星移到目标窗口上"));

    public ProcessPickerWindow()
    {
        InitializeComponent();
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    public string? SelectedProcessName { get; private set; }

    public int SelectedPid { get; private set; }

    private void Window_MouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
        UpdateTargetFromCursor();
        Point position = mouseEventArgs.GetPosition(this);
        double left = Math.Min(Math.Max(position.X + 18, 12), Math.Max(12, ActualWidth - HintBorder.ActualWidth - 12));
        double top = Math.Min(Math.Max(position.Y + 18, 12), Math.Max(12, ActualHeight - HintBorder.ActualHeight - 12));
        HintBorder.Margin = new Thickness(left, top, 0, 0);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        UpdateTargetFromCursor();
        if (string.IsNullOrWhiteSpace(SelectedProcessName))
        {
            return;
        }

        if (SelectedPid == Environment.ProcessId)
        {
            HintText = "不能选择当前程序";
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    private void UpdateTargetFromCursor()
    {
        if (!TryGetWindowTarget(out int pid, out string processName, out RectInt bounds))
        {
            SelectedPid = 0;
            SelectedProcessName = null;
            HintText = "将准星移到目标窗口上";
            HighlightBorder.Visibility = Visibility.Collapsed;
            return;
        }

        SelectedPid = pid;
        SelectedProcessName = processName;
        HintText = pid == Environment.ProcessId
            ? $"{processName}({pid})  当前程序，不能选择"
            : $"{processName}({pid})";
        UpdateHighlight(bounds);
    }

    private void UpdateHighlight(RectInt bounds)
    {
        Rect dip = ToDipRect(bounds);
        double left = dip.X - Left;
        double top = dip.Y - Top;
        double width = Math.Max(0, dip.Width);
        double height = Math.Max(0, dip.Height);
        if (width < 2 || height < 2)
        {
            HighlightBorder.Visibility = Visibility.Collapsed;
            return;
        }

        HighlightBorder.Margin = new Thickness(left, top, 0, 0);
        HighlightBorder.Width = width;
        HighlightBorder.Height = height;
        HighlightBorder.Visibility = Visibility.Visible;
    }

    private Rect ToDipRect(RectInt bounds)
    {
        Matrix transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        Point topLeft = transform.Transform(new Point(bounds.Left, bounds.Top));
        Point bottomRight = transform.Transform(new Point(bounds.Right, bounds.Bottom));
        return new Rect(topLeft, bottomRight);
    }

    private bool TryGetWindowTarget(out int pid, out string processName, out RectInt bounds)
    {
        pid = 0;
        processName = "";
        bounds = default;
        IntPtr overlay = new WindowInteropHelper(this).Handle;
        if (overlay == IntPtr.Zero || !GetCursorPos(out PointInt cursor))
        {
            return false;
        }

        IntPtr target = PeekWindowFromPoint(overlay, cursor);
        if (target == IntPtr.Zero)
        {
            return false;
        }

        target = GetAncestor(target, GaRoot);
        if (target == IntPtr.Zero || target == overlay)
        {
            return false;
        }

        GetWindowThreadProcessId(target, out uint processId);
        if (processId == 0)
        {
            return false;
        }

        pid = unchecked((int)processId);
        processName = ResolveProcessFileName(pid);
        if (string.IsNullOrWhiteSpace(processName) || !TryGetWindowBounds(target, out bounds))
        {
            return false;
        }

        return true;
    }

    private static IntPtr PeekWindowFromPoint(IntPtr overlay, PointInt cursor)
    {
        int style = GetWindowLong(overlay, GwlExStyle);
        SetWindowLong(overlay, GwlExStyle, style | WsExTransparent | WsExLayered);
        try
        {
            return WindowFromPoint(cursor);
        }
        finally
        {
            SetWindowLong(overlay, GwlExStyle, style);
        }
    }

    private static bool TryGetWindowBounds(IntPtr hwnd, out RectInt bounds)
    {
        if (DwmGetWindowAttribute(hwnd, DwmwaExtendedFrameBounds, out bounds, Marshal.SizeOf<RectInt>()) == 0
            && bounds.Right > bounds.Left
            && bounds.Bottom > bounds.Top)
        {
            return true;
        }

        return GetWindowRect(hwnd, out bounds);
    }

    private static string ResolveProcessFileName(int pid)
    {
        try
        {
            using Process process = Process.GetProcessById(pid);
            try
            {
                string? fileName = process.MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    return System.IO.Path.GetFileName(fileName);
                }
            }
            catch
            {
            }

            string name = process.ProcessName;
            return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name : $"{name}.exe";
        }
        catch
        {
            return "";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointInt
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RectInt
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out PointInt lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(PointInt point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RectInt lpRect);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RectInt pvAttribute, int cbAttribute);
}
