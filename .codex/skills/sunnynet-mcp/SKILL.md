---
name: "sunnynet-mcp"
description: "Use when working with the SunnyNet WPF MCP server to inspect captured HTTP/WebSocket/TCP/UDP sessions, query request or response bodies, manage favorites and notes, search/highlight traffic, control proxy/capture state, or configure SunnyNet rules from an AI session."
---

# SunnyNet MCP

Use this skill when a user asks the agent to operate SunnyNet through MCP: view captured requests, analyze a session, inspect response content, search traffic, manage favorites/notes, use WebSocket data, or control SunnyNet proxy/capture settings.

## Preconditions

- Cursor 配置指向 `SunnyNet.exe --mcp -port 29999`。主程序没开时，MCP 会自动拉起界面。
- MCP 默认随程序启动。不需要时可在底栏或设置里关闭。
- 不再需要单独的 `sunnynet-mcp.exe`。
- Tool namespace is usually `mcp__sunnynet__`.

If SunnyNet MCP tools are unavailable, tell the user to enable MCP in SunnyNet and check their client MCP config before trying again.

## Core Workflow

1. For traffic questions, start with `request_list` or `request_search`.
2. 界面 `#` 序号就是 `theology`。用户说“序号 5 / #5”时传 `theology=5`。
3. For one session, call `request_get` first, then call `request_get_response_body_decoded` only when body content is needed.
4. For HTTPS fingerprint questions, read `request_get(...).tlsFingerprint`; it contains JA3/JA3N/JA4 fields when the captured session has a TLS ClientHello.
5. For WebSocket/TCP/UDP, call `connection_list`, then `socket_data_list`, then `socket_data_get` or `socket_data_get_range`.
6. Prefer summaries for large bodies; do not paste huge responses unless the user explicitly asks.
7. Do not mutate or destroy data unless the user explicitly asks.

## Safety Rules

Ask for explicit confirmation before:

- `request_clear`, `request_delete`
- `proxy_stop`, `proxy_set_ie`, `proxy_unset_ie`, `proxy_set_port`
- `request_modify_body`, `response_modify_body`
- `request_modify_header`, `response_modify_header`
- `request_resend`, `request_block`, `request_release_all`
- `breakpoint_clear`, `replace_rules_clear`
- `request_clear_by_rule`, `request_rules_enable`, `request_rules_remove`

Favorites and notes are low-risk, but still follow the user's exact target session numbers.

## Common Tasks

### List current sessions

Use `request_list` with a small limit first:

```json
{ "limit": 20, "offset": 0 }
```

Return the important fields: `theology`（即界面序号）、`method`, `url`, `statusCode`, `length`, `notes`, `isFavorite`, `hasTagColor`.

### Analyze one session

1. `request_get` with `{ "theology": 5 }`
2. If needed, `request_get_response_body_decoded` with `{ "theology": 5 }`
3. Summarize request URL, method, headers, parameters, status, content type, and response meaning.

### Inspect TLS Fingerprints

Use `request_get` and read the `tlsFingerprint` object. It is present for HTTPS sessions where SunnyNet captured the client `ClientHello`; it is `null` for plain HTTP or records imported/captured before this field existed.

Important fields:

- `ja3Hash`, `ja3Text`, `ja3nHash`, `ja3nText`
- `ja4`, `ja4o`, `ja4r`, `ja4ro`
- `sni`, `alpn`, `legacyVersionText`, `highestVersionText`
- `cipherSuites`, `extensions`, `supportedGroups`, `signatureAlgorithms`

Example:

```json
{ "theology": 5 }
```

Then summarize `tlsFingerprint` rather than recalculating it from headers or body data.

### Work with favorites and notes

- List favorites: `request_favorites_list`
- Add favorite: `request_favorite_add` with `theology`
- Remove favorite: `request_favorite_remove` with `theology`
- List tagged sessions: `request_tags_list`
- Open tagged or specified sessions in the UI: `request_open`
- Open or activate the SunnyNet window: `app_open`
- List search highlights: `request_highlights_list`
- Set or clear notes: `request_notes_set`; pass empty `notes` to clear.
- Mark rows: `request_tag_set`; clear marks: `request_tag_clear`.

### Search and highlight traffic

- Search in URL/body: `request_body_search`
- Search only favorites: add `favoritesOnly: true`
- Highlight in UI: `request_highlight_search`
- Clear highlight: `request_highlight_clear`

### Breakpoints and intercept rules

Use `breakpoint_add` for URL regex breakpoints, then `breakpoint_list` to verify.

For intercepted sessions, inspect the current session before modifying or releasing. If the user wants to edit upstream/downstream data, prefer body/header modify tools only after they clearly specify the change.

For WPF rule center rules, prefer `request_rules_list`, `request_rules_enable`, `request_rules_remove`, and `request_rule_hits_list`. The older `replace_rules_*` and `breakpoint_*` tools are compatibility APIs.

## Detailed Tool Reference

For the complete tool list and parameters, read `references/tools.md`.
