using System.Collections.ObjectModel;
using System.Text;
using SunnyNet.Wpf.ViewModels;

namespace SunnyNet.Wpf.Models;

public sealed class CloudHookState : ViewModelBase
{
    private bool _enabled;
    private bool _running;
    private bool _starting;
    private string _device = "";
    private string _lastError = "";

    public ObservableCollection<string> Logs { get; } = new();

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (!SetProperty(ref _enabled, value))
            {
                return;
            }

            RaiseDerived();
        }
    }

    public bool Running
    {
        get => _running;
        set
        {
            if (!SetProperty(ref _running, value))
            {
                return;
            }

            if (value)
            {
                Starting = false;
            }

            RaiseDerived();
        }
    }

    public bool Starting
    {
        get => _starting;
        set
        {
            if (!SetProperty(ref _starting, value))
            {
                return;
            }

            RaiseDerived();
        }
    }

    public string Device
    {
        get => _device;
        set => SetProperty(ref _device, value ?? "");
    }

    public string LastError
    {
        get => _lastError;
        set
        {
            if (!SetProperty(ref _lastError, value ?? ""))
            {
                return;
            }

            RaiseDerived();
        }
    }

    public string ButtonText => Starting ? "启动中" : Running ? "抓包中" : "Cloud安卓";

    public string ToolTipText => Running
        ? "安卓云函数抓包中 (点击停止，右键查看日志)"
        : Starting
            ? "正在启动 Hook..."
            : "启动安卓云函数抓包 (右键查看日志)";

    public string FooterStatusText
    {
        get
        {
            if (Starting)
            {
                return "云函数抓包启动中";
            }

            if (Running)
            {
                return string.IsNullOrWhiteSpace(Device) ? "云函数抓包中" : $"云函数抓包中 · {Device}";
            }

            return string.IsNullOrWhiteSpace(LastError) ? "云函数抓包已关闭" : "云函数抓包失败";
        }
    }

    public string LogText
    {
        get
        {
            StringBuilder builder = new();
            foreach (string line in Logs)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
    }

    public void ClearLogs()
    {
        Logs.Clear();
        OnPropertyChanged(nameof(LogText));
    }

    public void AppendLog(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        Logs.Add(line.TrimEnd());
        while (Logs.Count > 400)
        {
            Logs.RemoveAt(0);
        }

        OnPropertyChanged(nameof(LogText));
    }

    private void RaiseDerived()
    {
        OnPropertyChanged(nameof(ButtonText));
        OnPropertyChanged(nameof(ToolTipText));
        OnPropertyChanged(nameof(FooterStatusText));
    }
}
