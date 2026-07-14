using System.Collections.ObjectModel;

namespace ALE.ACS.Presentation.Models;

public class TreeNode
{
    public string Name { get; set; } = string.Empty;

    public ProjectItemType Type { get; set; }

    public ObservableCollection<TreeNode> Children { get; } = new();

    public bool IsExpanded { get; set; }

    public bool IsSelected { get; set; }
}