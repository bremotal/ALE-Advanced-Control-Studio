using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Explorer;

public interface IProjectExplorerService
{
    IReadOnlyList<TreeNode> LoadProjectTree();
}