namespace Darp.Utils.Tests.Assets;

using System.Text.Json;
using Common;
using FluentAssertions;
using NSubstitute;
using Utils.Assets.Assets;

public sealed class AssetsExtensionTests
{
    private static readonly byte[] TestConfig = """
                                                {
                                                   "Name":"Test"
                                                }
                                                """u8.ToArray();

    [Fact]
    public async Task DeserializeJsonAsync_WhenCalled_ReturnsDeserializedObject()
    {
        // Arrange
        var readOnlyService = Substitute.For<IReadOnlyAssetsService>();
        var stream = new MemoryStream(TestConfig);
        readOnlyService.GetReadOnlySteam(Arg.Any<string>()).Returns(stream);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await readOnlyService.DeserializeJsonAsync<TestObject>("test.json", null, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task DeserializeJsonAsync_WhenDeserializationReturnsNull_ThrowsException()
    {
        // Arrange
        var readOnlyService = Substitute.For<IReadOnlyAssetsService>();
        readOnlyService.GetReadOnlySteam(Arg.Any<string>()).Returns(new MemoryStream());
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await readOnlyService.DeserializeJsonAsync<TestObject>("test.json", null, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SerializeJsonAsync_WhenCalled_SerializesObject()
    {
        // Arrange
        var writeOnlyService = Substitute.For<IWriteOnlyAssetsService>();
        var buffer = new byte[100];
        var stream = new MemoryStream(buffer);
        writeOnlyService.GetWriteOnlySteam(Arg.Any<string>()).Returns(stream);
        var testObject = new TestObject { Name = "Test" };
        var cancellationToken = CancellationToken.None;

        // Act
        await writeOnlyService.SerializeJsonAsync("test.json", testObject, null, cancellationToken);

        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        var resultObject = await JsonSerializer.DeserializeAsync<TestObject>(memoryStream, cancellationToken: cancellationToken);

        // Assert
        resultObject.Should().NotBeNull();
        resultObject?.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CopyToAsync_WhenCalled_CopiesStreamCorrectly()
    {
        // Arrange
        var sourceService = Substitute.For<IReadOnlyAssetsService>();
        var targetService = Substitute.For<IWriteOnlyAssetsService>();
        var sourceStream = new MemoryStream("Test data"u8.ToArray());
        var buffer = new byte[100];
        var targetStream = new MemoryStream(buffer);

        sourceService.GetReadOnlySteam(Arg.Any<string>()).Returns(sourceStream);
        targetService.GetWriteOnlySteam(Arg.Any<string>()).Returns(targetStream);

        // Act
        await sourceService.CopyToAsync("source.json", targetService, "target.json");

        // Assert
        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        memoryStream.Length.Should().BeGreaterThan(0);
        using var reader = new StreamReader(memoryStream);
        string content = await reader.ReadToEndAsync();
        content.Should().Be("Test data");
    }

    private sealed class TestObject
    {
        public required string Name { get; init; }
    }
}
