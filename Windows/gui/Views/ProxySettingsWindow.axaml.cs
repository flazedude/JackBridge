using Avalonia.Controls;
using Avalonia.Interactivity;
using JackBridge.GUI.ViewModels;

namespace JackBridge.GUI.Views;

public partial class ProxySettingsWindow : UserControl
{
    public ProxySettingsWindow()
    {
        InitializeComponent();

        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is ProxySettingsViewModel vm)
            {
                var comboBox = this.FindControl<ComboBox>("ProxyTypeComboBox");
                if (comboBox != null)
                {
                    foreach (var obj in comboBox.Items)
                    {
                        if (obj is ComboBoxItem item && item.Tag is string tag && tag == vm.ProxyType)
                        {
                            comboBox.SelectedItem = item;
                            break;
                        }
                    }

                    comboBox.SelectionChanged += (sender, args) =>
                    {
                        if (DataContext is ProxySettingsViewModel vm2)
                        {
                            if (comboBox.SelectedItem is ComboBoxItem sel && sel.Tag is string selTag)
                            {
                                vm2.ProxyType = selTag;
                            }
                        }
                    };
                }
            }
        };
    }
}
