using System.Windows;
using System.Windows.Controls;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Behaviors;

public static class TreeViewSelectionBehavior
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewSelectionBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static object? GetSelectedItem(DependencyObject element)
    {
        return element.GetValue(SelectedItemProperty);
    }

    public static void SetSelectedItem(DependencyObject element, object? value)
    {
        element.SetValue(SelectedItemProperty, value);
    }

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(TreeViewSelectionBehavior),
            new PropertyMetadata(false));

    private static bool GetIsUpdating(DependencyObject element)
    {
        return (bool)element.GetValue(IsUpdatingProperty);
    }

    private static void SetIsUpdating(DependencyObject element, bool value)
    {
        element.SetValue(IsUpdatingProperty, value);
    }

    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(TreeViewSelectionBehavior),
            new PropertyMetadata(false));

    private static bool GetIsHooked(DependencyObject element)
    {
        return (bool)element.GetValue(IsHookedProperty);
    }

    private static void SetIsHooked(DependencyObject element, bool value)
    {
        element.SetValue(IsHookedProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not TreeView treeView)
            return;

        if (!GetIsHooked(treeView))
        {
            treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            SetIsHooked(treeView, true);
        }

        if (GetIsUpdating(treeView) || e.NewValue is not TreeNode selectedNode)
            return;

        SelectTreeViewItem(treeView, selectedNode);
    }

    private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not TreeView treeView)
            return;

        SetIsUpdating(treeView, true);
        treeView.SetCurrentValue(SelectedItemProperty, e.NewValue);
        SetIsUpdating(treeView, false);
    }

    private static bool SelectTreeViewItem(ItemsControl parent, TreeNode selectedNode)
    {
        foreach (var item in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is not TreeViewItem treeViewItem)
                continue;

            if (ReferenceEquals(item, selectedNode))
            {
                treeViewItem.IsSelected = true;
                treeViewItem.BringIntoView();
                return true;
            }

            if (treeViewItem.Items.Count == 0)
                continue;

            treeViewItem.IsExpanded = true;

            if (SelectTreeViewItem(treeViewItem, selectedNode))
                return true;
        }

        return false;
    }
}