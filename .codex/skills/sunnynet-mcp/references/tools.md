# SunnyNet MCP Tool Reference

This is a compact reference for the current SunnyNet WPF MCP implementation. Full project documentation also exists at `docs/MCP工具清单.md`.

Cursor / Claude 配置示例：`SunnyNet.exe --mcp -port 29999`。界面没开时会自动拉起主程序。

## Session Identity

- 界面 `#` 序号就是 `theology`。用户说“序号 12 / #12”时传 `theology=12`。
- `index` 与 `theology` 是同一个数字，可互换。
- 批量工具用 `theologies`（同样是界面序号列表）。

## App

- `app_open()`：打开或激活 SunnyNet 主窗口。主程序未运行时，`SunnyNet.exe --mcp` 会先自动启动界面。

## Traffic List

- `request_list(limit, offset, favoritesOnly, taggedOnly, highlightedOnly, tagColor)`
- `request_search(url, method, status_code, favoritesOnly, taggedOnly, highlightedOnly, tagColor, limit, offset)`
- `request_stats()`
- `request_get(theology)`
- `request_get_response_body_decoded(theology)`
- `request_body_search(keyword, scope, favoritesOnly, limit)`
- `request_highlight_search(keyword, type, color)`
- `request_highlight_clear()`
- `request_tags_list(limit, offset, tagColor)`
- `request_open(theology, theologies, taggedOnly)`
- `request_highlights_list(limit, offset)`

`scope`: `url`, `request_body`, `response_body`, `all`.

`request_get` returns `tlsFingerprint` for HTTPS sessions with captured TLS ClientHello data. It is `null` when no TLS fingerprint exists. Key fields:

- `ja3Hash`, `ja3Text`, `ja3nHash`, `ja3nText`
- `ja4`, `ja4o`, `ja4r`, `ja4ro`
- `sni`, `alpn`, `legacyVersion`, `legacyVersionText`, `highestVersion`, `highestVersionText`
- `cipherSuites`, `extensions`, `supportedGroups`, `ecPointFormats`, `signatureAlgorithms`
- `rawClientHelloHex`

## Favorites and Notes

- `request_favorites_list(limit, offset)`
- `request_favorite_add(theology)`
- `request_favorite_remove(theology)`
- `request_notes_set(theology, notes)`
- `request_tag_set(theology|theologies, color)`
- `request_tag_clear(theology|theologies)`

Empty `notes` clears the note.

`color` accepts normal WPF color strings such as `#FF6A00`; `__strike__` marks a strikethrough row.

## Session Mutation

- `request_modify_header(theology, key, value)`
- `request_modify_body(theology, body)`
- `response_modify_header(theology, key, value)`
- `response_modify_body(theology, body)`
- `request_resend(theology|theologies)`
- `request_block(theology)`
- `request_release_all()`
- `request_delete(theology|theologies)`
- `request_clear()`
- `request_clear_by_rule(rule)`
- `request_save_all(path)`
- `request_import(path)`

Use these only after the user explicitly asks.

`request_clear_by_rule` supports `all`, `resources`, `no_response`, `failed`, `redirect`, `unfavorite`, `untagged`, `current_filter`.

## Long Connections

- `connection_list(protocol, favoritesOnly, limit, offset)`
- `socket_data_list(theology, limit, offset)`
- `socket_data_get(theology, index)` — `index` 是数据包序号
- `socket_data_get_range(theology, start, end)`

`protocol`: `websocket`, `tcp`, `udp`.

## Proxy and Capture

- `proxy_get_status()`
- `proxy_start()`
- `proxy_stop()`
- `proxy_set_port(port)`
- `proxy_set_ie()`
- `proxy_unset_ie()`
- `proxy_pause_capture()`
- `proxy_resume_capture()`

## Rules and Settings

- `config_get()`
- `request_rules_list(type)`
- `request_rules_enable(type, hash, enabled)`
- `request_rules_remove(type, hash)`
- `request_rule_hits_list(theology, ruleType, limit, offset)`
- `hosts_list()`, `hosts_add(source, target)`, `hosts_remove(index)`
- `replace_rules_list()`, `replace_rules_add(type, source, target)`, `replace_rules_remove(hash|index)`, `replace_rules_clear()`
- `breakpoint_add(url_pattern)`, `breakpoint_list()`, `breakpoint_remove(index)`, `breakpoint_clear()`
- `process_list()`, `process_add_name(name)`, `process_remove_name(name)`
- `cert_install()`, `cert_export(path)`

Rule `type`: `all`, `http_block`, `websocket_block`, `tcp_block`, `udp_block`, `rewrite`, `mapping`.
`replace_rules_*` and `breakpoint_*` are compatibility APIs; prefer `request_rules_*` for the WPF rule center.

Replace rule `type`: `Base64`, `HEX`, `String(UTF8)`, `String(GBK)`, `响应文件`.

## Response Field Hints

Session list/detail results usually include:

- `index`
- `theology`
- `method`
- `url`
- `statusCode`
- `pid`
- `clientIP`
- `sendTime`
- `recTime`
- `way`
- `host`
- `query`
- `tlsFingerprint`
- `length`
- `type`
- `process`
- `tagColor`
- `hasTagColor`
- `breakMode`
- `ruleHitCount`
- `ruleHitSummary`
- `notes`
- `isFavorite`
