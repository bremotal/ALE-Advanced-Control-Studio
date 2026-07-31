using FluentAssertions;
using ALE.ACS.Presentation.Services.Documents;

namespace ALE.ACS.Presentation.Tests;

public class DocumentServiceTests
{
    [Fact]
    public void OpenDocument_AddsDocumentAndSetsItActive()
    {
        var service = new DocumentService();

        service.OpenDocument("Spec", "Content", "Engineering");

        service.Documents.Should().ContainSingle();
        service.ActiveDocument.Should().NotBeNull();
        service.ActiveDocument!.Title.Should().Be("Spec");
        service.ActiveDocument.Kind.Should().Be("Engineering");
    }

    [Fact]
    public void CloseDocument_RemovesActiveDocumentAndClearsSelectionWhenNeeded()
    {
        var service = new DocumentService();
        service.OpenDocument("First", "Content");
        service.OpenDocument("Second", "Content");

        service.CloseDocument(service.ActiveDocument!.Id);

        service.Documents.Should().ContainSingle();
        service.ActiveDocument.Should().NotBeNull();
        service.ActiveDocument!.Title.Should().Be("First");
    }
}
