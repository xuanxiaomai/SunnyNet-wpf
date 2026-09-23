using SunnyNet.Wpf.ViewModels;

namespace SunnyNet.Wpf.Models;

public sealed class InterceptListItem : ViewModelBase
{
    private int _index;
    private string _contains = "包含";
    private string _target = "";
    private string _interceptType = "HTTP屏蔽";
    private string _suspendProcess = "否";
    private string _state = "未保存";

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string Contains
    {
        get => _contains;
        set => SetProperty(ref _contains, string.IsNullOrWhiteSpace(value) ? "包含" : value);
    }

    public string Target
    {
        get => _target;
        set => SetProperty(ref _target, value ?? "");
    }

    public string InterceptType
    {
        get => _interceptType;
        set => SetProperty(ref _interceptType, string.IsNullOrWhiteSpace(value) ? "HTTP屏蔽" : value);
    }

    public string SuspendProcess
    {
        get => _suspendProcess;
        set => SetProperty(ref _suspendProcess, string.IsNullOrWhiteSpace(value) ? "否" : value);
    }

    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value ?? "");
    }

    public object? Source { get; init; }

    public static InterceptListItem FromBreakpoint(InterceptRuleItem item, int index)
    {
        return new InterceptListItem
        {
            Index = index,
            Contains = item.Operator,
            Target = string.IsNullOrWhiteSpace(item.Value) ? item.Name : item.Value,
            InterceptType = RuleDataKinds.Normalize(item.DataKind),
            SuspendProcess = "是",
            State = item.State,
            Source = item
        };
    }

    public static InterceptListItem FromBlock(TrafficRuleItemBase item, string interceptType, int index)
    {
        return new InterceptListItem
        {
            Index = index,
            Contains = item.UrlMatchType,
            Target = string.IsNullOrWhiteSpace(item.UrlPattern) ? item.Name : item.UrlPattern,
            InterceptType = string.IsNullOrWhiteSpace(item.DataKind) ? interceptType : RuleDataKinds.Normalize(item.DataKind),
            SuspendProcess = "否",
            State = item.State,
            Source = item
        };
    }
}
