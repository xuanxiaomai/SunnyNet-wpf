namespace SunnyNet.Wpf.Models;

public static class RuleDataKinds
{
    public const string Default = "HTTP请求-URL";

    public static readonly string[] All =
    [
        "HTTP请求-URL",
        "HTTP请求-Header",
        "HTTP请求-Header-UA",
        "HTTP请求-Header-Cookie",
        "HTTP请求-Body",
        "HTTP响应-Header",
        "HTTP响应-Body",
        "HTTP请求和响应-所有",
        "发送-WS",
        "接收-WS",
        "发送和接收-WS",
        "连接-更改远程地址-TCP",
        "发送-TCP",
        "接收-TCP",
        "发送和接收-TCP",
        "发送-UDP",
        "接收-UDP",
        "发送和接收-UDP",
        "所有"
    ];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Default;
        }

        foreach (string item in All)
        {
            if (string.Equals(item, value, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return value switch
        {
            "HTTP屏蔽" or "请求断点" => Default,
            "WebSocket屏蔽" => "发送和接收-WS",
            "TCP屏蔽" => "发送和接收-TCP",
            "UDP屏蔽" => "发送和接收-UDP",
            _ => Default
        };
    }

    public static string ToInterceptCategory(string? value)
    {
        string kind = Normalize(value);
        if (kind.Contains("WS", StringComparison.Ordinal))
        {
            return "WebSocket屏蔽";
        }

        if (kind.Contains("TCP", StringComparison.Ordinal))
        {
            return "TCP屏蔽";
        }

        if (kind.Contains("UDP", StringComparison.Ordinal))
        {
            return "UDP屏蔽";
        }

        return "HTTP屏蔽";
    }
}
