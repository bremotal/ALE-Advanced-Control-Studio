using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Explorer;

public interface IProjectExplorerService
{
    ObservableCollection<TreeNode> ProjectItems { get; }

    void AddModel(string modelName);

    void AddController(string controllerName);
}