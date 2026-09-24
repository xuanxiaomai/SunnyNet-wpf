using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SunnyNet.Wpf.Services;

internal static class McpStdioHost
{
    public static bool IsRequested(IReadOnlyList<string> args)
    {
        foreach (string arg in args)
        {
            if (string.Equals(arg, "--mcp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "-mcp", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static void Run(IReadOnlyList<string> args)
    {
        int port = ReadPort(args, SunnyNetCompatibleMcpServer.DefaultPort);
        string endpoint = $"http://127.0.0.1:{port}/mcp";
        string healthUrl = $"http://127.0.0.1:{port}/mcp/health";
        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(30) };
        EnsureGuiRunning(client, healthUrl);
        using Stream stdin = Console.OpenStandardInput();
        using Stream stdout = Console.OpenStandardOutput();
        using StreamReader reader = new(stdin, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4 * 1024 * 1024, leaveOpen: true);
        using StreamWriter writer = new(stdout, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true,
            NewLine = "\n"
        };

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                if (TryHandleLocal(line, writer))
                {
                    continue;
                }
            }
            catch (JsonException)
            {
            }

            try
            {
                using HttpResponseMessage response = client.PostAsync(
                    endpoint,
                    new StringContent(line, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().TrimEnd('\r', '\n', ' ');
                if (body.Length > 0)
                {
                    writer.WriteLine(body);
                }
            }
            catch (Exception exception)
            {
                if (EnsureGuiRunning(client, healthUrl))
                {
                    try
                    {
                        using HttpResponseMessage retry = client.PostAsync(
                            endpoint,
                            new StringContent(line, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                        string retryBody = retry.Content.ReadAsStringAsync().GetAwaiter().GetResult().TrimEnd('\r', '\n', ' ');
                        if (retryBody.Length > 0)
                        {
                            writer.WriteLine(retryBody);
                            continue;
                        }
                    }
                    catch (Exception retryException)
                    {
                        exception = retryException;
                    }
                }

                WriteError(writer, ReadJsonId(line), -32603, $"连接SunnyNet失败: {exception.Message}");
            }
        }
    }

    private static bool TryHandleLocal(string line, StreamWriter writer)
    {
        using JsonDocument document = JsonDocument.Parse(line);
        JsonElement root = document.RootElement;
        string method = root.TryGetProperty("method", out JsonElement methodElement)
            ? methodElement.GetString() ?? ""
            : "";
        object? id = ReadJsonId(root);

        if (string.Equals(method, "initialize", StringComparison.Ordinal))
        {
            WriteJson(writer, new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new
                    {
                        tools = new { }
                    },
                    serverInfo = new
                    {
                        name = "sunnynet",
                        version = "1.0.0"
                    }
                }
            });
            return true;
        }

        if (string.Equals(method, "notifications/initialized", StringComparison.Ordinal)
            || string.Equals(method, "initialized", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static bool EnsureGuiRunning(HttpClient client, string healthUrl)
    {
        if (IsHealthOk(client, healthUrl))
        {
            return true;
        }

        if (!HasOtherSunnyNetProcess())
        {
            return false;
        }

        return WaitForHealth(client, healthUrl, TimeSpan.FromSeconds(20));
    }

    private static bool IsHealthOk(HttpClient client, string healthUrl)
    {
        try
        {
            using HttpResponseMessage response = client.GetAsync(healthUrl).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static bool WaitForHealth(HttpClient client, string healthUrl, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (IsHealthOk(client, healthUrl))
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    private static bool HasOtherSunnyNetProcess()
    {
        int currentId = Environment.ProcessId;
        foreach (Process process in Process.GetProcessesByName("SunnyNet"))
        {
            if (process.Id != currentId)
            {
                return true;
            }
        }

        return false;
    }

    private static int ReadPort(IReadOnlyList<string> args, int defaultPort)
    {
        for (int index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], "-port", StringComparison.OrdinalIgnoreCase)
                || string.Equals(args[index], "--port", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(args[index + 1], out int port) && port is > 0 and <= 65535)
                {
                    return port;
                }
            }
        }

        return defaultPort;
    }

    private static object? ReadJsonId(string line)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(line);
            return ReadJsonId(document.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadJsonId(JsonElement root)
    {
        if (!root.TryGetProperty("id", out JsonElement id))
        {
            return null;
        }

        return id.ValueKind switch
        {
            JsonValueKind.Number when id.TryGetInt64(out long number) => number,
            JsonValueKind.String => id.GetString(),
            _ => null
        };
    }

    private static void WriteError(StreamWriter writer, object? id, int code, string message)
    {
        WriteJson(writer, new
        {
            jsonrpc = "2.0",
            id,
            error = new
            {
                code,
                message
            }
        });
    }

    private static void WriteJson(StreamWriter writer, object value)
    {
        writer.WriteLine(JsonSerializer.Serialize(value));
    }
}
