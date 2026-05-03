using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Input;
using JackBridge.GUI.ViewModels;

namespace JackBridge.GUI.Views;

public class SelectAllTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool allSelected && allSelected ? "Deselect All" : "Select All";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SelectAllIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool allSelected && allSelected ? "☑" : "☐";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class ProxyRulesWindow : UserControl
{
    private static readonly DataFormat<string> DraggedRuleIdFormat =
        DataFormat.CreateStringApplicationFormat("jackbridge.dragged-rule-id");

    private bool _isUpdatingFromViewModel = false;
    private Border? _draggingRow;
    private Border? _dropTargetRow;

    public ProxyRulesWindow()
    {
        InitializeComponent();

        if (this.FindControl<ComboBox>("ProtocolComboBox") is ComboBox protocolComboBox)
        {
            protocolComboBox.SelectionChanged += ProtocolComboBox_SelectionChanged;
        }

        this.DataContextChanged += ProxyRulesWindow_DataContextChanged;

        RegisterRuleListDragHandlers("ActiveRulesItemsControl");
        RegisterRuleListDragHandlers("StaticRulesItemsControl");
    }

    private void RegisterRuleListDragHandlers(string controlName)
    {
        if (this.FindControl<ItemsControl>(controlName) is ItemsControl itemsControl)
        {
            itemsControl.AddHandler(DragDrop.DropEvent, Rules_Drop);
            itemsControl.AddHandler(DragDrop.DragOverEvent, Rules_DragOver);
        }
    }

    private void ProxyRulesWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ProxyRulesViewModel vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;

            UpdateComboBoxSelections(vm);
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ProxyRulesViewModel vm)
        {
            if (e.PropertyName == nameof(ProxyRulesViewModel.NewProtocol))
            {
                UpdateProtocolComboBox(vm.NewProtocol);
            }
            else if (e.PropertyName == nameof(ProxyRulesViewModel.NewProxyAction))
            {
                UpdateActionComboBox(vm.NewProxyAction);
            }
        }
    }

    private void UpdateComboBoxSelections(ProxyRulesViewModel vm)
    {
        UpdateProtocolComboBox(vm.NewProtocol);
        UpdateActionComboBox(vm.NewProxyAction);
    }

    private void UpdateProtocolComboBox(string protocol)
    {
        if (this.FindControl<ComboBox>("ProtocolComboBox") is ComboBox protocolComboBox)
        {
            _isUpdatingFromViewModel = true;

            foreach (var item in protocolComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem &&
                    comboBoxItem.Tag is string tag &&
                    tag.Equals(protocol, StringComparison.OrdinalIgnoreCase))
                {
                    protocolComboBox.SelectedItem = comboBoxItem;
                    break;
                }
            }

            _isUpdatingFromViewModel = false;
        }
    }

    private void UpdateActionComboBox(string action)
    {
        if (this.FindControl<ComboBox>("ActionComboBox") is ComboBox actionComboBox)
        {
            _isUpdatingFromViewModel = true;

            foreach (var item in actionComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem &&
                    comboBoxItem.Tag is string tag &&
                    tag.Equals(action, StringComparison.OrdinalIgnoreCase))
                {
                    actionComboBox.SelectedItem = comboBoxItem;
                    break;
                }
            }

            _isUpdatingFromViewModel = false;
        }
    }

    private void ActionComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // dont update ViewModel when updating from Viewmodel
        if (_isUpdatingFromViewModel)
            return;

        if (sender is ComboBox comboBox &&
            comboBox.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag &&
            DataContext is ProxyRulesViewModel vm)
        {
            vm.NewProxyAction = tag;
        }
    }

    private void ProtocolComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFromViewModel)
            return;

        if (sender is ComboBox comboBox &&
            comboBox.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag &&
            DataContext is ProxyRulesViewModel vm)
        {
            vm.NewProtocol = tag;
        }
    }

    private async void Rule_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.DataContext is not ProxyRule rule)
            return;

        SetDraggingRow(border);

        var dragData = new DataTransfer();
        dragData.Add(DataTransferItem.Create(
            DraggedRuleIdFormat,
            rule.RuleId.ToString(CultureInfo.InvariantCulture)));

        var result = await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Move);
        ClearDragVisuals();

        if (result == DragDropEffects.Move && DataContext is ProxyRulesViewModel vm)
        {
            for (int i = 0; i < vm.ProxyRules.Count; i++)
            {
                vm.ProxyRules[i].Index = i + 1;
            }
        }
    }

    private void Rules_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Move;
        SetDropTargetRow(FindRuleRow(e.Source as Control));
    }

    private void Rules_Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is not ProxyRulesViewModel vm)
            return;

        string? draggedRuleIdText = e.DataTransfer.TryGetValue(DraggedRuleIdFormat);
        if (!uint.TryParse(draggedRuleIdText, NumberStyles.None, CultureInfo.InvariantCulture, out uint draggedRuleId))
            return;

        var draggedRule = vm.ProxyRules.FirstOrDefault(rule => rule.RuleId == draggedRuleId);
        if (draggedRule == null)
            return;

        ClearDropTargetRow();

        if (e.Source is Control control)
        {
            if (FindRuleRow(control) is Border border && border.DataContext is ProxyRule targetRule)
            {
                if (draggedRule.RuleId == targetRule.RuleId)
                    return;

                if (draggedRule.IsStatic != targetRule.IsStatic)
                    return;

                var sectionRules = vm.ProxyRules
                    .Where(rule => rule.IsStatic == draggedRule.IsStatic)
                    .ToList();
                int draggedIndex = sectionRules.IndexOf(draggedRule);
                int targetIndex = sectionRules.IndexOf(targetRule);

                if (draggedIndex == -1 || targetIndex == -1 || draggedIndex == targetIndex)
                    return;

                vm.MoveRuleToIndex(draggedRule, targetIndex);
            }
        }
    }

    private static Border? FindRuleRow(Control? control)
    {
        var current = control;
        while (current != null)
        {
            if (current is Border border &&
                border.Classes.Contains("ruleRow") &&
                border.DataContext is ProxyRule)
            {
                return border;
            }

            current = current.Parent as Control;
        }

        return null;
    }

    private void SetDraggingRow(Border border)
    {
        _draggingRow?.Classes.Remove("dragging");
        _draggingRow = border;
        _draggingRow.Classes.Add("dragging");
    }

    private void SetDropTargetRow(Border? border)
    {
        if (_dropTargetRow == border)
            return;

        ClearDropTargetRow();

        if (border == null || border == _draggingRow)
            return;

        _dropTargetRow = border;
        _dropTargetRow.Classes.Add("dropTarget");
    }

    private void ClearDropTargetRow()
    {
        _dropTargetRow?.Classes.Remove("dropTarget");
        _dropTargetRow = null;
    }

    private void ClearDragVisuals()
    {
        _draggingRow?.Classes.Remove("dragging");
        _draggingRow = null;
        ClearDropTargetRow();
    }
}
