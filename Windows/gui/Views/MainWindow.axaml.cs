using Avalonia.Controls;
using JackBridge.GUI.ViewModels;
using System;
using System.ComponentModel;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace JackBridge.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set window reference in ViewModel
        this.Opened += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SetMainWindow(this);
            }
        };
    }

    private void OnChangeLanguageEnglish(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ChangeLanguage("en");
        }
    }

    private void OnChangeLanguageChinese(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ChangeLanguage("zh");
        }
    }

    private void OnPanelBackdropPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.ClosePanelCommand.CanExecute(null))
        {
            vm.ClosePanelCommand.Execute(null);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (e.CloseReason == WindowCloseReason.ApplicationShutdown)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
            base.OnClosing(e);
            return;
        }

        // verify if user cclose app or minimize to tray
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CloseToTray)
            {
                // minimize to tray
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                // exit the app if not tray
                viewModel.Cleanup();
                base.OnClosing(e);
            }
        }
        else
        {
            // fallback to tray
            e.Cancel = true;
            this.Hide();
        }
    }
}
