using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JackBridge.GUI.ViewModels;

namespace JackBridge.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Opened += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SetMainWindow(this);
                vm.NavigateToHome();
            }
        };
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnChangeLanguageEnglish(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.ChangeLanguage("en");
    }

    private void OnChangeLanguageChinese(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.ChangeLanguage("zh");
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.CloseToTray)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        if (DataContext is MainWindowViewModel vm2)
            vm2.Cleanup();

        base.OnClosing(e);
    }
}
