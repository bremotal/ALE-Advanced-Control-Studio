using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ALE.ACS.Presentation.Models;

public class TreeNode : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private ProjectItemType _type;
    private bool _isExpanded;
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ProjectItemType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public ObservableCollection<TreeNode> Children { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    protected virtual bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }
}