using System;
using System.Reflection;
using System.Windows.Input;
using JackBridge.GUI.Common;

namespace JackBridge.GUI.ViewModels;

public class AboutViewModel
{
    public string Version { get; }
    public ICommand CloseCommand { get; }

    public AboutViewModel() : this(() => { })
    {
    }

    public AboutViewModel(Action onClose)
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        Version = string.IsNullOrWhiteSpace(version)
            ? "Version 1.1.0-beta"
            : $"Version {version}";

        CloseCommand = new RelayCommand(onClose);
    }
}
