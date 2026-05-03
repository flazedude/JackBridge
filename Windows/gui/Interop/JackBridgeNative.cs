using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JackBridge.GUI.Interop;

public static class JackBridgeNative
{
    private const string DllName = "JackBridgeCore.dll";

    static JackBridgeNative()
    {
        var nativePath = Path.Combine(AppContext.BaseDirectory, "native");
        if (Directory.Exists(nativePath))
        {
            SetDllDirectory(nativePath);
            NativeLibrary.SetDllImportResolver(typeof(JackBridgeNative).Assembly, ResolveNativeLibrary);
        }
        else
        {
            var dllPath = Path.Combine(AppContext.BaseDirectory, DllName);
            if (File.Exists(dllPath))
            {
                NativeLibrary.Load(dllPath);
            }
        }
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!libraryName.Equals(DllName, StringComparison.OrdinalIgnoreCase) &&
            !libraryName.Equals(Path.GetFileNameWithoutExtension(DllName), StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        var dllPath = Path.Combine(AppContext.BaseDirectory, "native", DllName);
        return File.Exists(dllPath) && NativeLibrary.TryLoad(dllPath, out var handle)
            ? handle
            : IntPtr.Zero;
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string lpPathName);

    public enum ProxyType
    {
        HTTP = 0,
        SOCKS5 = 1
    }

    public enum RuleAction
    {
        PROXY = 0,
        DIRECT = 1,
        BLOCK = 2
    }

    public enum RuleProtocol
    {
        TCP = 0,
        UDP = 1,
        BOTH = 2
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback([MarshalAs(UnmanagedType.LPStr)] string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ConnectionCallback(
        [MarshalAs(UnmanagedType.LPStr)] string processName,
        uint pid,
        [MarshalAs(UnmanagedType.LPStr)] string destIp,
        ushort destPort,
        [MarshalAs(UnmanagedType.LPStr)] string proxyInfo);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JackBridge_AddRule(
        [MarshalAs(UnmanagedType.LPStr)] string processName,
        [MarshalAs(UnmanagedType.LPStr)] string targetHosts,
        [MarshalAs(UnmanagedType.LPStr)] string targetPorts,
        RuleProtocol protocol,
        RuleAction action);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_EnableRule(uint ruleId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_DisableRule(uint ruleId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_DeleteRule(uint ruleId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_EditRule(
        uint ruleId,
        [MarshalAs(UnmanagedType.LPStr)] string processName,
        [MarshalAs(UnmanagedType.LPStr)] string targetHosts,
        [MarshalAs(UnmanagedType.LPStr)] string targetPorts,
        RuleProtocol protocol,
        RuleAction action);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JackBridge_GetRulePosition(uint ruleId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_MoveRuleToPosition(uint ruleId, uint newPosition);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_SetProxyConfig(
        ProxyType type,
        [MarshalAs(UnmanagedType.LPStr)] string proxyIp,
        ushort proxyPort,
        [MarshalAs(UnmanagedType.LPStr)] string username,
        [MarshalAs(UnmanagedType.LPStr)] string password);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JackBridge_SetLogCallback(LogCallback callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JackBridge_SetConnectionCallback(ConnectionCallback callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JackBridge_SetTrafficLoggingEnabled([MarshalAs(UnmanagedType.Bool)] bool enable);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JackBridge_SetDnsViaProxy([MarshalAs(UnmanagedType.Bool)] bool enable);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JackBridge_SetLocalhostViaProxy([MarshalAs(UnmanagedType.Bool)] bool enable);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_Start();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JackBridge_Stop();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int JackBridge_TestConnection(
        [MarshalAs(UnmanagedType.LPStr)] string targetHost,
        ushort targetPort,
        [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder resultBuffer,
        UIntPtr bufferSize);
}
