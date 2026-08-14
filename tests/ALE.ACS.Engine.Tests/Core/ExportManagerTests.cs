using ALE.ACS.Engine.Core.Export;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class ExportManagerTests
{
    [Fact]
    public void Export_ShouldUseProviderThatCanHandleTheRequest()
    {
        var provider = new TestExportProvider();
        var manager = new ExportManager([provider]);

        var result = manager.Export(new ExportRequest(null, null, null, ExportFormat.Json));

        result.Success.Should().BeTrue();
        result.Artifacts.Should().ContainSingle();
    }

    private sealed class TestExportProvider : IExportProvider
    {
        public bool CanExport(ExportRequest request) => request.Format == ExportFormat.Json;

        public ExportResult Export(ExportRequest request)
            => new(true, [new ExportArtifact("artifact", ".json", "{}", "utf-8", new Dictionary<string, string>())], "ok");

        public ExportResult Validate(ExportRequest request)
            => new(true, Array.Empty<ExportArtifact>(), "ok");
    }
}
