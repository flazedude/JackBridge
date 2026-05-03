using System;
using JackBridge.GUI.Interop;

namespace JackBridge.GUI.Services;

public class JackBridgeService : IDisposable
{
    private JackBridgeNative.LogCallback? _logCallback;
    private JackBridgeNative.ConnectionCallback? _connectionCallback;
    private bool _isRunning;

    public event Action<string>? LogReceived;
    public event Action<string, uint, string, ushort, string>? ConnectionReceived;

    public JackBridgeService()
    {
        _logCallback = OnLogReceived;
        _connectionCallback = OnConnectionReceived;

        JackBridgeNative.JackBridge_SetLogCallback(_logCallback);
        JackBridgeNative.JackBridge_SetConnectionCallback(_connectionCallback);
    }

    private void OnLogReceived(string message)
    {
        LogReceived?.Invoke(message);
    }

    private void OnConnectionReceived(string processName, uint pid, string destIp, ushort destPort, string proxyInfo)
    {
        ConnectionReceived?.Invoke(processName, pid, destIp, destPort, proxyInfo);
    }

    public bool Start()
    {
        if (_isRunning)
            return true;

        _isRunning = JackBridgeNative.JackBridge_Start();
        return _isRunning;
    }

    public bool Stop()
    {
        if (!_isRunning)
            return true;

        _isRunning = !JackBridgeNative.JackBridge_Stop();
        return !_isRunning;
    }

    public bool SetProxyConfig(string type, string ip, ushort port, string username, string password)
    {
        var proxyType = type.ToUpper() == "HTTP"
            ? JackBridgeNative.ProxyType.HTTP
            : JackBridgeNative.ProxyType.SOCKS5;

        return JackBridgeNative.JackBridge_SetProxyConfig(proxyType, ip, port, username, password);
    }

    public uint AddRule(string processName, string targetHosts, string targetPorts, string protocol, string action)
    {
        var ruleAction = action.ToUpper() switch
        {
            "DIRECT" => JackBridgeNative.RuleAction.DIRECT,
            "BLOCK" => JackBridgeNative.RuleAction.BLOCK,
            _ => JackBridgeNative.RuleAction.PROXY
        };

        var ruleProtocol = protocol.ToUpper() switch
        {
            "UDP" => JackBridgeNative.RuleProtocol.UDP,
            "BOTH" => JackBridgeNative.RuleProtocol.BOTH,
            "TCP+UDP" => JackBridgeNative.RuleProtocol.BOTH,
            _ => JackBridgeNative.RuleProtocol.TCP
        };

        return JackBridgeNative.JackBridge_AddRule(processName, targetHosts, targetPorts, ruleProtocol, ruleAction);
    }

    public bool EnableRule(uint ruleId)
    {
        return JackBridgeNative.JackBridge_EnableRule(ruleId);
    }

    public bool DisableRule(uint ruleId)
    {
        return JackBridgeNative.JackBridge_DisableRule(ruleId);
    }

    public bool DeleteRule(uint ruleId)
    {
        return JackBridgeNative.JackBridge_DeleteRule(ruleId);
    }

    public bool EditRule(uint ruleId, string processName, string targetHosts, string targetPorts, string protocol, string action)
    {
        var ruleAction = action.ToUpper() switch
        {
            "DIRECT" => JackBridgeNative.RuleAction.DIRECT,
            "BLOCK" => JackBridgeNative.RuleAction.BLOCK,
            _ => JackBridgeNative.RuleAction.PROXY
        };

        var ruleProtocol = protocol.ToUpper() switch
        {
            "UDP" => JackBridgeNative.RuleProtocol.UDP,
            "BOTH" => JackBridgeNative.RuleProtocol.BOTH,
            "TCP+UDP" => JackBridgeNative.RuleProtocol.BOTH,
            _ => JackBridgeNative.RuleProtocol.TCP
        };

        return JackBridgeNative.JackBridge_EditRule(ruleId, processName, targetHosts, targetPorts, ruleProtocol, ruleAction);
    }

    public uint GetRulePosition(uint ruleId)
    {
        return JackBridgeNative.JackBridge_GetRulePosition(ruleId);
    }

    public bool MoveRuleToPosition(uint ruleId, uint newPosition)
    {
        return JackBridgeNative.JackBridge_MoveRuleToPosition(ruleId, newPosition);
    }

    public void SetDnsViaProxy(bool enable)
    {
        JackBridgeNative.JackBridge_SetDnsViaProxy(enable);
    }

    public void SetLocalhostViaProxy(bool enable)
    {
        JackBridgeNative.JackBridge_SetLocalhostViaProxy(enable);
    }

    public static void SetTrafficLoggingEnabled(bool enable)
    {
        JackBridgeNative.JackBridge_SetTrafficLoggingEnabled(enable);
    }

    public string TestConnection(string targetHost, ushort targetPort)
    {
        var buffer = new System.Text.StringBuilder(4096);
        int result = JackBridgeNative.JackBridge_TestConnection(
            targetHost,
            targetPort,
            buffer,
            (UIntPtr)buffer.Capacity);

        return buffer.ToString();
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            Stop(); // removing the threads, C code handle close no need to manually handle drives
        }
        GC.SuppressFinalize(this);
    }
}
