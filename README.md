# JackBridge

JackBridge is a Windows-focused custom build based on [ProxyBridge](https://github.com/InterceptSuite/ProxyBridge) by InterceptSuite. It keeps the original idea of routing selected application traffic through a proxy, then adds a cleaner JackBridge desktop workflow, portable configuration, rule priority, and day-to-day usability improvements.

This repository is based on ProxyBridge and continues to credit the original project and authors. JackBridge is an improved/customized version maintained by Jack Wang.

## What JackBridge Does

JackBridge lets you route traffic from specific Windows processes through an HTTP or SOCKS5 proxy while leaving other applications direct. It uses a native Windows interception layer with WinDivert and a desktop UI for managing proxy settings, process rules, and live activity.

Common examples:

- Route `chrome.exe` through a SOCKS5 proxy.
- Keep `Codex.exe` direct while proxying everything else.
- Add rules from observed active processes.
- Use a portable folder that can be moved between machines.

## JackBridge v1.5 Highlights

- Rebranded ProxyBridge into JackBridge.
- Portable `config.json` stored beside `JackBridge.exe`.
- Clean single-file app layout with native dependencies in `native\`.
- Multiple JackBridge instances allowed, so beta builds do not hijack the stable daily driver.
- Main proxy on/off control changed into a graphical switch.
- Added Rules and Settings shortcut buttons to the main toolbar.
- Removed the update-check UI.
- Updated About text to: `Based on: ProxyBridge, improved by Jack Wang`.
- Added custom JackBridge app icon and proxy/on-off assets.
- Added process-rule priority ordering.
- Split rules into `Active rules` and `Static rules`.
- Active rules are handled before static rules.
- Rule priority starts at `1` inside each section.
- Improved process rules UI with a dedicated popup window.
- Improved responsiveness so action buttons do not fall off-screen.
- Added process discovery from activity so observed processes can be added as rules quickly.
- Deduplicated observed active processes.
- Kept traffic/activity logging focused inside the main UI.

## Changes Compared With ProxyBridge

ProxyBridge provided the original cross-platform proxy routing foundation. JackBridge v1.5 focuses on making the Windows desktop experience easier to use as a daily driver.

Major differences:

- **Branding:** JackBridge name, title, icon, package names, and About text.
- **Portability:** configuration is stored in the executable folder instead of AppData.
- **Packaging:** release folder is simplified to `JackBridge.exe` plus subfolders.
- **Rule priority:** top rules are handled first, making direct exceptions possible before global proxy rules.
- **Rule sections:** active rules are prioritized above static/global rules.
- **UI workflow:** rules/settings use cleaner panels or popup windows instead of scattered separate settings boxes.
- **Proxy toggle:** graphical on/off switch with generated icons.
- **Process selection:** active/observed processes can be turned into rules more quickly.
- **Multiple instances:** JackBridge-specific builds can run side by side without flashing an already-running instance.
- **Update removal:** update-check UI was removed for a cleaner custom build.

## Portable Layout

The v1.5 package is designed to be copied as a folder:

```text
JackBridge v1.5\
  JackBridge.exe
  config.json
  native\
    JackBridgeCore.dll
    WinDivert.dll
    WinDivert64.sys
    WinDivert-LICENSE.txt
```

`config.json` is created after first run. Copying the folder to another machine brings the proxy settings and rules with it.

## Rule Priority Model

Rules are evaluated from top to bottom.

Example:

```text
Active rules
  1. Codex.exe -> DIRECT

Static rules
  1. * -> PROXY
```

In this setup, `Codex.exe` stays direct while the static catch-all rule sends everything else through the configured proxy.

Rule sections:

- **Active rules:** specific app/process rules. These are handled first.
- **Static rules:** always-on or broader rules, such as global `*` proxy routing.

## Building

Requirements:

- Windows
- .NET 10 SDK
- MinGW/GCC for building the native Windows core
- WinDivert files under `Windows\vendor\WinDivert-2.2.2-A\`

Build the GUI:

```powershell
dotnet build Windows\gui\JackBridge.GUI.csproj --no-restore
```

Publish a self-contained Windows build:

```powershell
dotnet publish Windows\gui\JackBridge.GUI.csproj -c Release -r win-x64 --self-contained true --no-restore -o "build\JackBridge v1.5" /p:PublishAot=false /p:PublishTrimmed=false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:DebugType=None /p:DebugSymbols=false
```

Build the native core:

```powershell
gcc -shared -O2 -DJACKBRIDGE_EXPORTS -o "build\JackBridge v1.5\native\JackBridgeCore.dll" "Windows\src\JackBridge.c" -I"Windows\src" -I"Windows\vendor\WinDivert-2.2.2-A\include" -L"Windows\vendor\WinDivert-2.2.2-A\x64" -lWinDivert -lws2_32 -liphlpapi -lpsapi
```

Then copy:

```text
Windows\vendor\WinDivert-2.2.2-A\x64\WinDivert.dll
Windows\vendor\WinDivert-2.2.2-A\x64\WinDivert64.sys
Windows\vendor\WinDivert-2.2.2-A\LICENSE
```

into:

```text
build\JackBridge v1.5\native\
```

## Credits

JackBridge is based on [ProxyBridge](https://github.com/InterceptSuite/ProxyBridge) by InterceptSuite.

Original project:

- ProxyBridge: https://github.com/InterceptSuite/ProxyBridge
- Original authors/maintainers: InterceptSuite

JackBridge custom improvements:

- Improved by Jack Wang
- JackBridge rebrand, portable Windows packaging, rule priority, UI cleanup, and workflow changes

## License

This project is derived from ProxyBridge. See the repository license and retain upstream attribution when redistributing modified builds.
