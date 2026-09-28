using SunnyNet.Wpf.ViewModels;

namespace SunnyNet.Wpf.Models;

public sealed class UpstreamProxyItem : ViewModelBase
{
    private int _displayIndex;
    private string _hash = Guid.NewGuid().ToString("N");
    private string _type = "s5";
    private string _address = "";
    private string _port = "";
    private string _user = "";
    private string _password = "";
    private string _remark = "";
    private bool _isEnabled;

    public int DisplayIndex
    {
        get => _displayIndex;
        set => SetProperty(ref _displayIndex, value);
    }

    public string Hash
    {
        get => _hash;
        set => SetProperty(ref _hash, string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString("N") : value);
    }

    public string Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, NormalizeType(value)))
            {
                OnPropertyChanged(nameof(TypeText));
            }
        }
    }

    public string TypeText => IsHttp ? "http代理" : "s5代理";

    public bool IsHttp => string.Equals(_type, "http", StringComparison.OrdinalIgnoreCase);

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value ?? "");
    }

    public string Port
    {
        get => _port;
        set => SetProperty(ref _port, value ?? "");
    }

    public string User
    {
        get => _user;
        set => SetProperty(ref _user, value ?? "");
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value ?? "");
    }

    public string Remark
    {
        get => _remark;
        set => SetProperty(ref _remark, value ?? "");
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(ref _isEnabled, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public string StatusText => _isEnabled ? "启用" : "禁用";

    public string ToProxyUrl()
    {
        return BuildProxyUrl(IsHttp ? "http" : "s5", Address, Port, User, Password);
    }

    public static string NormalizeType(string? type)
    {
        return string.Equals(type, "http", StringComparison.OrdinalIgnoreCase) ? "http" : "s5";
    }

    public static string BuildProxyUrl(string type, string address, string port, string user, string password)
    {
        string scheme = NormalizeType(type) == "http" ? "http" : "socket5";
        string host = (address ?? "").Trim();
        string hostPort = string.IsNullOrWhiteSpace(port) ? host : $"{host}:{port.Trim()}";
        if (string.IsNullOrWhiteSpace(user) && string.IsNullOrWhiteSpace(password))
        {
            return $"{scheme}://{hostPort}";
        }

        return $"{scheme}://{user}:{password}@{hostPort}";
    }

    public static bool IsEmptyProxyUrl(string? url)
    {
        string value = (url ?? "").Trim();
        return string.IsNullOrWhiteSpace(value) ||
               string.Equals(value, "socket5://:@", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "http://:@", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryParse(string? input, out bool isHttp, out string address, out string port, out string user, out string password)
    {
        isHttp = false;
        address = "";
        port = "";
        user = "";
        password = "";

        string text = (input ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text) || IsEmptyProxyUrl(text))
        {
            return false;
        }

        string lower = text.ToLowerInvariant();
        if (lower.StartsWith("http://", StringComparison.Ordinal) || lower.StartsWith("https://", StringComparison.Ordinal))
        {
            isHttp = true;
        }
        else if (lower.StartsWith("socks", StringComparison.Ordinal) ||
                 lower.StartsWith("socket", StringComparison.Ordinal) ||
                 lower.StartsWith("s5://", StringComparison.Ordinal))
        {
            isHttp = false;
        }

        if (TryParseAsUri(text, out address, out port, out user, out password))
        {
            return !string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(port);
        }

        int at = text.LastIndexOf('@');
        if (at > 0)
        {
            string account = text[..at];
            string hostPart = text[(at + 1)..];
            SplitAccount(account, out user, out password);
            return TrySplitHostPort(hostPart, out address, out port);
        }

        string[] parts = text.Split(':');
        if (parts.Length >= 4 && !text.Contains("://", StringComparison.Ordinal))
        {
            address = parts[0].Trim();
            port = parts[1].Trim();
            user = parts[2];
            password = string.Join(":", parts.Skip(3));
            return !string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(port);
        }

        return TrySplitHostPort(text, out address, out port);
    }

    private static bool TryParseAsUri(string text, out string address, out string port, out string user, out string password)
    {
        address = "";
        port = "";
        user = "";
        password = "";

        string candidate = text;
        if (!candidate.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        candidate = candidate
            .Replace("socket5://", "socks5://", StringComparison.OrdinalIgnoreCase)
            .Replace("socket://", "socks5://", StringComparison.OrdinalIgnoreCase)
            .Replace("s5://", "socks5://", StringComparison.OrdinalIgnoreCase);

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? uri) || string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        address = uri.Host;
        port = uri.Port > 0 && uri.Port != 80 && uri.Port != 443
            ? uri.Port.ToString()
            : ExtractExplicitPort(uri);
        SplitAccount(Uri.UnescapeDataString(uri.UserInfo), out user, out password);
        return !string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(port);
    }

    private static string ExtractExplicitPort(Uri uri)
    {
        string authority = uri.Authority;
        int index = authority.LastIndexOf(':');
        if (index <= 0 || index >= authority.Length - 1)
        {
            return uri.Port > 0 ? uri.Port.ToString() : "";
        }

        string value = authority[(index + 1)..];
        return int.TryParse(value, out _) ? value : (uri.Port > 0 ? uri.Port.ToString() : "");
    }

    private static void SplitAccount(string account, out string user, out string password)
    {
        int index = account.IndexOf(':');
        if (index < 0)
        {
            user = account;
            password = "";
            return;
        }

        user = account[..index];
        password = account[(index + 1)..];
    }

    private static bool TrySplitHostPort(string text, out string address, out string port)
    {
        address = "";
        port = "";
        string value = text.Trim();
        int index = value.LastIndexOf(':');
        if (index <= 0 || index >= value.Length - 1)
        {
            return false;
        }

        address = value[..index].Trim().Trim('[', ']');
        port = value[(index + 1)..].Trim();
        return !string.IsNullOrWhiteSpace(address) && int.TryParse(port, out int number) && number is > 0 and <= 65535;
    }
}
