using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JackBridge.GUI.Common;

namespace JackBridge.GUI.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private bool _isProxyEnabled;
    private string _proxyStatusText = "Proxy Off";
    private string _proxyStatusColor = "#FFE81123";
    private string _engineType = "External";
    private int _activeRulesCount;
    private string _recentActivity = "";

    public bool IsProxyEnabled
    {
        get => _isProxyEnabled;
        set
        {
            SetProperty(ref _isProxyEnabled, value);
            ProxyStatusText = value ? "Proxy On" : "Proxy Off";
            ProxyStatusColor = value ? "#FF147A45" : "#FFE81123";
        }
    }

    public string ProxyStatusText
    {
        get => _proxyStatusText;
        set => SetProperty(ref _proxyStatusText, value);
    }

    public string ProxyStatusColor
    {
        get => _proxyStatusColor;
        set => SetProperty(ref _proxyStatusColor, value);
    }

    public string EngineType
    {
        get => _engineType;
        set => SetProperty(ref _engineType, value);
    }

    public int ActiveRulesCount
    {
        get => _activeRulesCount;
        set => SetProperty(ref _activeRulesCount, value);
    }

    public string RecentActivity
    {
        get => _recentActivity;
        set => SetProperty(ref _recentActivity, value);
    }

    public ICommand ToggleProxyCommand { get; }

    public HomeViewModel(
        ICommand toggleProxyCommand)
    {
        ToggleProxyCommand = toggleProxyCommand;
    }
}
