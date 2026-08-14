using ALE.ACS.Engine.Core.Applications;
using ALE.ACS.Engine.Core.Projects;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class ProjectRepositoryTests
{
    [Fact]
    public void SaveLoadDelete_ShouldRoundTripProjectSnapshot()
    {
        var provider = new InMemoryPersistenceProvider();
        var manager = new PersistenceManager([provider]);
        var repository = new ProjectRepository(manager);
        var project = new EngineeringProject(Guid.NewGuid(), "Demo", "Demo project", "1.0", [], new Dictionary<string, string>());
        var snapshot = new ProjectSnapshot(project.Id.ToString(), project.Name, project.Version, DateTime.UtcNow, DateTime.UtcNow, "payload", new Dictionary<string, string>());

        var saveResult = repository.Save(project, new SaveOptions("default"));
        saveResult.Success.Should().BeTrue();

        var loadResult = repository.Load(project.Id.ToString(), new LoadOptions("default"));
        loadResult.Success.Should().BeTrue();
        loadResult.Snapshot.Should().NotBeNull();

        var deleteResult = repository.Delete(project.Id.ToString(), new DeleteOptions("default"));
        deleteResult.Success.Should().BeTrue();
    }

    private sealed class InMemoryPersistenceProvider : IPersistenceProvider
    {
        private readonly Dictionary<string, ProjectSnapshot> _storage = new();

        public RepositoryResult Save(ProjectSnapshot snapshot, SaveOptions options)
        {
            _storage[snapshot.Id] = snapshot;
            return RepositoryResult.CreateSuccess();
        }

        public RepositoryResult Load(string id, LoadOptions options)
        {
            return _storage.TryGetValue(id, out var snapshot)
                ? RepositoryResult.CreateSuccess(snapshot)
                : RepositoryResult.CreateFailure("Not found");
        }

        public bool Exists(string id) => _storage.ContainsKey(id);

        public RepositoryResult Delete(string id, DeleteOptions options)
        {
            _storage.Remove(id);
            return RepositoryResult.CreateSuccess();
        }

        public RepositoryResult Validate(ProjectSnapshot snapshot)
        {
            return snapshot.Id.Length > 0 ? RepositoryResult.CreateSuccess() : RepositoryResult.CreateFailure("Invalid snapshot");
        }
    }
}
