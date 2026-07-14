using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Projects;

public class FakeProjectService
{
    public ObservableCollection<TreeNode> LoadProject()
    {
        return
        [
            new TreeNode
            {
                Name = "ALE Demo Project",
                Type = ProjectItemType.Project,
                IsExpanded = true,

                Children =
                {
                    CreateControllers(),
                    CreateModels(),
                    CreateIO(),
                    CreateFolder("Tags", ProjectItemType.Tag),
                    CreateFolder("Scripts", ProjectItemType.Script),
                    CreateFolder("Trends", ProjectItemType.Trend),
                    CreateFolder("Historian", ProjectItemType.Historian),
                    CreateFolder("Reports", ProjectItemType.Report)
                }
            }
        ];
    }

    private static TreeNode CreateControllers()
    {
        return new TreeNode
        {
            Name = "Controllers",
            Type = ProjectItemType.Folder,
            IsExpanded = true,

            Children =
            {
                new TreeNode { Name = "Fuzzy Controller", Type = ProjectItemType.Controller },
                new TreeNode { Name = "MPC Controller", Type = ProjectItemType.Controller },
                new TreeNode { Name = "Adaptive MPC", Type = ProjectItemType.Controller }
            }
        };
    }

    private static TreeNode CreateModels()
    {
        return new TreeNode
        {
            Name = "Models",

            Type = ProjectItemType.Folder,

            Children =
            {
                new TreeNode { Name = "Distillation", Type = ProjectItemType.Model },
                new TreeNode { Name = "Evaporator", Type = ProjectItemType.Model },
                new TreeNode { Name = "Boiler", Type = ProjectItemType.Model }
            }
        };
    }

    private static TreeNode CreateIO()
    {
        return new TreeNode
        {
            Name = "IO",

            Type = ProjectItemType.IO,

            Children =
            {
                new TreeNode { Name = "Inputs", Type = ProjectItemType.IO },
                new TreeNode { Name = "Outputs", Type = ProjectItemType.IO }
            }
        };
    }

    private static TreeNode CreateFolder(string name, ProjectItemType type)
    {
        return new TreeNode
        {
            Name = name,
            Type = type
        };
    }
}