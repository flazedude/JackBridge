# JackBridge Handoff Notes

This repository is a custom fork of ProxyBridge, renamed JackBridge.

## Origin And Credit

- Upstream base: `https://github.com/InterceptSuite/ProxyBridge.git`
- Product name in this fork: JackBridge
- README should continue to credit ProxyBridge clearly.
- About box text requested by user: `Based on: ProxyBridge, improved by Jack Wang`

## Released Baseline

JackBridge v1.5 is the user's current daily driver.

- Pushed to GitHub: `https://github.com/flazedude/JackBridge.git`
- Branch/main and tag `v1.5` were pushed earlier.
- Daily driver install path observed during debugging:
  `C:\Users\flaze\Desktop\JackBridge v1.5\JackBridge.exe`
- v1.5 is often running while v2 beta is being tested. Do not kill it or launch/stop apps unless the user explicitly asks.

## Current Test Build

The v2 beta debug build is here:

`C:\Users\flaze\Desktop\Coding\jackproxy\Windows\gui\bin\Debug\net10.0-windows\JackBridge.exe`

Native DLL used by that build:

`C:\Users\flaze\Desktop\Coding\jackproxy\Windows\gui\bin\Debug\net10.0-windows\JackBridgeCore.dll`

Current config path:

`C:\Users\flaze\Desktop\Coding\jackproxy\Windows\gui\bin\Debug\net10.0-windows\config.json`

Important user instruction: do not launch the GUI for the user unless explicitly asked. Tell them the path instead.

## Version 1.5 Work Completed

Major v1.5 changes over ProxyBridge:

- Renamed app to JackBridge.
- Portable config stored beside the executable.
- Unified settings/rules UI moved away from multiple detached settings windows.
- Graphical Proxy On/Off switch, defaulting off for clean installs.
- Process Rules UI:
  - Priority ordering.
  - Active rules and Static rules sections.
  - Top rules evaluate first.
  - Static catch-all rules can act as fallback.
  - Add process from running process list.
  - Add rule from Connections/activity observed processes.
  - Import/export/delete selected.
- Connections/activity view deduplicates active processes better than original.
- About box cleaned up.
- Menu buttons added for Rules and Settings.
- Build layout cleaned so most support files live in subfolders.

## Version 2.0 Beta Goal

Add optional built-in proxy support using mihomo, while keeping external proxy mode.

User requirements:

- Switch between External proxy and Built-in proxy.
- Built-in proxy should be portable.
- Users can provide:
  - Subscription URL.
  - Local YAML profile by file picker/drag-and-drop/import.
  - Routing mode/preset.
  - DNS options.
  - GEO database update options.
  - Mixed port, default `8888`.
- Users can select an actual proxy server from a loaded profile, not just proxy groups.
- Include mihomo executable, profiles, GEO assets, and config in beta packages.

## Current v2 Source Areas

Most relevant files:

- `Windows/gui/Services/ConfigManager.cs`
  - App config model.
  - `BuiltInProxyConfig`.
- `Windows/gui/Services/MihomoService.cs`
  - Install/update mihomo core.
  - Import/refresh YAML profiles.
  - Write `core\runtime.yaml`.
  - Start/stop mihomo.
  - Parse proxy servers from YAML/profile/controller.
- `Windows/gui/Services/WindowsProcessJob.cs`
  - Windows job object so mihomo is killed if JackBridge dies.
- `Windows/gui/ViewModels/MainWindowViewModel.cs`
  - Proxy toggle flow.
  - Settings/rules windows.
  - Built-in start/stop and health timer.
  - Rule synchronization into native core.
- `Windows/gui/ViewModels/ProxySettingsViewModel.cs`
  - Built-in proxy settings UI state and commands.
- `Windows/gui/Views/ProxySettingsWindow.axaml`
  - Built-in proxy UI.
- `Windows/gui/ViewModels/ProxyRulesViewModel.cs`
  - Process rule add/edit/delete/priority UI behavior.
- `Windows/src/JackBridge.c`
  - Native transparent proxy engine.
  - WinDivert packet capture/redirect.
  - Native rule evaluation.
  - Local TCP/UDP relay.

## Recent Fixes Already Applied

### Rule delete bug

User reported: deleting a rule confirmed but did not remove it.

Fix in `ProxyRulesViewModel.cs`:

- Delete no longer requires `_proxyService.DeleteRule(rule.RuleId)` to succeed.
- Config/UI collection is now source of truth.
- Native delete is best effort only when `RuleId > 0`.

Same fix applied to delete-selected.

### Add rule from Connections

Earlier behavior depended on native `AddRule` returning an ID, so adding a rule could fail while proxy/native service was not running.

Fix in `MainWindowViewModel.cs`:

- Add rule to JackBridge config/UI first.
- Best-effort native sync via `TrySyncRuleToNative`.
- Native sync happens again when proxy starts and rules are loaded.

### Proxy settings save crash

Save while proxy is on was moved to background task and guarded with error handling. On failure, proxy flips off.

### Built-in mihomo startup

Mihomo should not start at app launch just because settings are opened. It starts as part of Proxy On.

### Built-in process cleanup

`WindowsProcessJob` attaches mihomo to a kill-on-job-close object. If JackBridge crashes/exits, child mihomo should terminate.

### Runtime YAML duplicate key handling

`MihomoService.WriteRuntimeConfig` removes managed top-level keys from the user profile before appending JackBridge-controlled values, avoiding duplicate YAML keys.

### Server selection

Current direction: show actual proxy servers only in selector UI. Groups are used internally for applying the selection, but the UI should not primarily list group-to-group entries.

### Native loop guards

`Windows/src/JackBridge.c` now hard-bypasses known proxy core processes before rules:

- `mihomo.exe`
- `jackbridge-mihomo.exe`
- `verge-mihomo.exe`
- `clash-verge.exe`
- `clash-core-service.exe`
- `clash-core-service`

`MainWindowViewModel.AddBuiltInBypassRules()` also injects DIRECT rules for those processes and common local proxy ports.

## Current Critical Issue / Latest Decision

User reports v2 hangs immediately after manually clicking Proxy On when v1.5 daily driver is already running. Latest decision: do not try to run two JackBridge transparent redirectors at the same time. v2 must refuse to enable transparent mode while another JackBridge instance is active, then log a clear message and flip back Off instead of hanging.

Live system facts observed:

- v1.5 JackBridge is running:
  `C:\Users\flaze\Desktop\JackBridge v1.5\JackBridge.exe`
- v1.5 owns JackBridge native relay ports:
  - TCP `0.0.0.0:34010`
  - UDP `0.0.0.0:34011`
- Clash Verge is running:
  - `C:\Program Files\Clash Verge\clash-verge.exe`
  - `C:\Program Files\Clash Verge\verge-mihomo.exe`
  - `clash-core-service`
- Clash Verge/mihomo listens on:
  - `127.0.0.1:7897`
  - DNS `0.0.0.0:53`
- v1.5 config routes `verge-mihomo.exe` DIRECT, but not `mihomo.exe`.

Likely causes / findings:

1. Definite old bug: v2 native engine used the same hardcoded relay ports as v1.5: `34010/34011`.
2. Possible coexistence issue: two WinDivert transparent redirectors running simultaneously can conflict even after relay ports are unique.
3. Possible old double-proxy issue: v1.5 could catch v2 built-in `mihomo.exe` because v1.5 only bypasses `verge-mihomo.exe`.
4. Bad experiment reverted: launching JackBridge's built-in mihomo as `verge-mihomo.exe` made it indistinguishable from Clash Verge's real core and worsened debugging.

Recent patches for this:

- `Windows/src/JackBridge.c`
  - Added dynamic relay port selection. v2 scans from `34010` upward and should skip ports owned by v1.5, usually choosing `34012/34013`.
  - Added `g_local_udp_relay_port` instead of hardcoded `LOCAL_UDP_RELAY_PORT` in runtime packet flow/filter.
  - Added native bypass for other JackBridge relay ports in the `34010..34209` range so v2 should pass v1.5 relay traffic through.
- `Windows/gui/Services/MihomoService.cs`
  - Do not launch JackBridge's built-in mihomo as `verge-mihomo.exe`; that is Clash Verge's core name and confused debugging.
  - If core path is `mihomo.exe`, v2 copies/launches it as `core\jackbridge-mihomo.exe` so it is visibly JackBridge-owned.
  - Note: v1.5 will not automatically bypass `jackbridge-mihomo.exe` unless its config is updated. Prefer testing v2 with v1.5 Proxy Off, or add a DIRECT rule for `jackbridge-mihomo.exe` to v1.5 if coexistence is required.
- `Windows/gui/ViewModels/MainWindowViewModel.cs`
  - Added `IsAnotherJackBridgeInstanceRunning()`.
  - `StartProxyService()` now refuses to start if another `JackBridge.exe` is running from a different directory.
  - Expected Activity log when v1.5 is active:
    `ERROR: Another JackBridge instance is already running. Turn off/exit v1.5 before enabling v2 transparent proxy.`
  - It also logs:
    `Built-in mihomo was not started, to avoid WinDivert and relay-port conflicts.`
  - This guard is intentional. It is the current practical fix to avoid hangs during testing.

These changes were rebuilt into the debug app folder:

- `dotnet build Windows\gui\JackBridge.GUI.csproj --no-restore` passed.
- Native DLL rebuilt manually with gcc into debug output.
- Stale debug copy `core\verge-mihomo.exe` was removed. JackBridge should not create that file again.

Testing expectation:

- With v1.5 running, clicking v2 Proxy On should no longer hang. It should refuse to start and flip/log off.
- To actually test v2 transparent routing, v1.5 must be turned off/exited first.
- If v2 still hangs even with the guard and v1.5 running, inspect whether the guard is executing before `MihomoService.StartAsync()` and native `JackBridge_Start()`.
- If v2 hangs with v1.5 off, focus on `MihomoService.StartAsync()`, `JackBridge_Start()`, and WinDivert open/startup.

## Build Commands

Normal GUI build:

```powershell
dotnet build Windows\gui\JackBridge.GUI.csproj --no-restore
```

Manual native DLL rebuild used in this environment:

```powershell
gcc -shared -O2 -flto -s -Wall -D_WIN32_WINNT=0x0601 -DJACKBRIDGE_EXPORTS -I"Windows\vendor\WinDivert-2.2.2-A\include" Windows\src\JackBridge.c -L"Windows\vendor\WinDivert-2.2.2-A\x64" -lWinDivert -lws2_32 -liphlpapi -o Windows\gui\bin\Debug\net10.0-windows\JackBridgeCore.dll
```

Expected GCC warnings are currently:

- Ignored MSVC `#pragma comment`.
- Unused local `src_port` in packet processor.
- Unused local `from_ip` in UDP relay server.

Those warnings are not new blockers.

## Debugging Commands Used

Check relevant processes:

```powershell
Get-Process | Where-Object { $_.ProcessName -match 'mihomo|verge|JackBridge|Clash|WinDivert|tun|proxy' } | Select-Object Id,ProcessName,Path,Responding,CPU,StartTime | Sort-Object ProcessName
```

Check local proxy/relay ports:

```powershell
netstat -ano | Select-String ':34010|:34011|:34012|:34013|:8888|:9090|:7890|:7891|:7892|:7897|:53'
```

Check v1.5 config:

```powershell
Get-Content "C:\Users\flaze\Desktop\JackBridge v1.5\config.json"
```

## Recommended Next Steps

1. Ask user to retest the current debug build after the latest dynamic relay port/native DLL rebuild.
2. If it still hangs, ask whether they can temporarily turn v1.5 Proxy Off before turning v2 On.
3. If v2 works with v1.5 off, the remaining issue is dual WinDivert coexistence.
4. If v2 still hangs with v1.5 off, focus on:
   - `MihomoService.StartAsync`
   - `JackBridge_Start`
   - WinDivert open/startup path
   - blocking waits in native start/stop
5. Consider adding explicit UI warning when another JackBridge instance is running with Proxy On.
6. Consider exposing native relay ports in config for advanced debugging.

## Important User Preferences

- User does not want v2 beta to affect v1.5 daily driver.
- User wants v2 named/versioned as beta until stable.
- User wants portable layout.
- User wants clean UI, no awkward grey space, no extra settings windows when avoidable.
- User wants practical testing paths and builds, but asked not to have Codex launch the app for them.
