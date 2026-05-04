using System.Collections.ObjectModel;
using System.Windows.Input;
using JackBridge.GUI.Common;

namespace JackBridge.GUI.ViewModels;

public class LogEntry : ViewModelBase
{
    private string _timestamp = "";
    private string _level = "INFO";
    private string _message = "";
    private string _levelColor = "#FF9E9E9E";

    public string Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }

    public string Level
    {
        get => _level;
        set
        {
            SetProperty(ref _level, value);
            LevelColor = value switch
            {
                "ERROR" => "#FFE81123",
                "WARN" => "#FFFFB900",
                _ => "#FF9E9E9E"
            };
        }
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string LevelColor
    {
        get => _levelColor;
        set => SetProperty(ref _levelColor, value);
    }
}

public class ActivityViewModel : ViewModelBase
{
    private string _searchText = "";
    private bool _hasEntries;
    private bool _hasNoEntries = true;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool HasEntries
    {
        get => _hasEntries;
        set => SetProperty(ref _hasEntries, value);
    }

    public bool HasNoEntries
    {
        get => _hasNoEntries;
        set => SetProperty(ref _hasNoEntries, value);
    }

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand AddRuleCommand { get; }

    public ActivityViewModel(
        ICommand searchCommand,
        ICommand clearCommand,
        ICommand addRuleCommand)
    {
        SearchCommand = searchCommand;
        ClearCommand = clearCommand;
        AddRuleCommand = addRuleCommand;

        LogEntries.CollectionChanged += (_, _) =>
        {
            HasEntries = LogEntries.Count > 0;
            HasNoEntries = LogEntries.Count == 0;
        };
    }
}
