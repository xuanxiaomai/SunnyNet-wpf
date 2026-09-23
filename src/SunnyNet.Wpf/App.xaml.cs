using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using SunnyNet.Wpf.Services;

namespace SunnyNet.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        ForceContextSubmenusOpenRight();

        if (McpStdioHost.IsRequested(eventArgs.Args))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(eventArgs);
            McpStdioHost.Run(eventArgs.Args);
            Shutdown(0);
            return;
        }

        base.OnStartup(eventArgs);

        try
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            LogException("Startup", exception);
            MessageBox.Show(exception.Message, "SunnyNet 启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void ForceContextSubmenusOpenRight()
    {
        SetMenuDropAlignmentRight();
        SystemParameters.StaticPropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SystemParameters.MenuDropAlignment))
            {
                SetMenuDropAlignmentRight();
            }
        };

        EventManager.RegisterClassHandler(
            typeof(MenuItem),
            MenuItem.SubmenuOpenedEvent,
            new RoutedEventHandler(OnMenuItemSubmenuOpened));
    }

    private static void SetMenuDropAlignmentRight()
    {
        if (!SystemParameters.MenuDropAlignment)
        {
            return;
        }

        typeof(SystemParameters)
            .GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, false);
    }

    private static void OnMenuItemSubmenuOpened(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not MenuItem menuItem
            || menuItem.Role is MenuItemRole.TopLevelHeader or MenuItemRole.TopLevelItem)
        {
            return;
        }

        menuItem.ApplyTemplate();
        if (menuItem.Template?.FindName("PART_Popup", menuItem) is not Popup popup)
        {
            return;
        }

        popup.Placement = PlacementMode.Right;
        popup.HorizontalOffset = 0;
        popup.VerticalOffset = 0;
    }

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        LogException("Dispatcher", eventArgs.Exception);
        MessageBox.Show(eventArgs.Exception.Message, "SunnyNet 运行异常", MessageBoxButton.OK, MessageBoxImage.Error);
        eventArgs.Handled = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        if (eventArgs.ExceptionObject is Exception exception)
        {
            LogException("Unhandled", exception);
        }
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs)
    {
        LogException("TaskScheduler", eventArgs.Exception);
        eventArgs.SetObserved();
    }

    private static void LogException(string source, Exception exception)
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirectory = Path.Combine(appData, "SunnyNet", "logs");
            Directory.CreateDirectory(logDirectory);
            string logPath = Path.Combine(logDirectory, "crash.log");
            File.AppendAllText(
                logPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
