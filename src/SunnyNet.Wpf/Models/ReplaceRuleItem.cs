using SunnyNet.Wpf.ViewModels;

namespace SunnyNet.Wpf.Models;

public sealed class ReplaceRuleItem : ViewModelBase
{
    private int _index;
    private string _hash = Guid.NewGuid().ToString("N");
    private string _matchTarget = "";
    private string _dataKind = RuleDataKinds.Default;
    private string _ruleType = "String(UTF8)";
    private string _sourceContent = "";
    private string _replacementContent = "";
    private string _state = "未保存";
    private bool _replaceAll;
    private bool _useHex;

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string MatchTarget
    {
        get => _matchTarget;
        set
        {
            if (SetProperty(ref _matchTarget, value ?? ""))
            {
                State = "未保存";
                OnPropertyChanged(nameof(DisplayTarget));
            }
        }
    }

    public string DisplayTarget => string.IsNullOrWhiteSpace(MatchTarget) ? "所有请求" : MatchTarget;

    public string DataKind
    {
        get => _dataKind;
        set
        {
            if (SetProperty(ref _dataKind, RuleDataKinds.Normalize(value)))
            {
                State = "未保存";
            }
        }
    }

    public string Hash
    {
        get => _hash;
        set => SetProperty(ref _hash, string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString("N") : value);
    }

    public string RuleType
    {
        get => _ruleType;
        set
        {
            if (SetProperty(ref _ruleType, string.IsNullOrWhiteSpace(value) ? "String(UTF8)" : value))
            {
                State = "未保存";
                OnPropertyChanged(nameof(UsesResponseFile));
            }
        }
    }

    public string SourceContent
    {
        get => _sourceContent;
        set
        {
            if (SetProperty(ref _sourceContent, value ?? ""))
            {
                State = "未保存";
            }
        }
    }

    public string ReplacementContent
    {
        get => _replacementContent;
        set
        {
            if (SetProperty(ref _replacementContent, value ?? ""))
            {
                State = "未保存";
            }
        }
    }

    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value ?? "");
    }

    public bool ReplaceAll
    {
        get => _replaceAll;
        set
        {
            if (SetProperty(ref _replaceAll, value))
            {
                State = "未保存";
            }
        }
    }

    public bool UseHex
    {
        get => _useHex;
        set
        {
            if (SetProperty(ref _useHex, value))
            {
                State = "未保存";
            }
        }
    }

    public bool UsesResponseFile => RuleType == "响应文件";
}
