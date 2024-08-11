namespace Darp.Utils.Tests.Assets;

using System.Text.Json;
using Common;
using FluentAssertions;
using NSubstitute;
using Utils.Assets;
using Utils.Assets.Abstractions;

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
        IReadOnlyAssetsService readOnlyService = Substitute.For<IReadOnlyAssetsService>();
        var stream = new MemoryStream(TestConfig);
        readOnlyService.GetReadOnlySteam(Arg.Any<string>()).Returns(stream);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        TestObject result = await readOnlyService.DeserializeJsonAsync<TestObject>("test.json", cancellationToken: cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task DeserializeJsonAsync_WhenDeserializationReturnsNull_ThrowsException()
    {
        // Arrange
        IReadOnlyAssetsService readOnlyService = Substitute.For<IReadOnlyAssetsService>();
        readOnlyService.GetReadOnlySteam(Arg.Any<string>()).Returns(new MemoryStream());
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await readOnlyService.DeserializeJsonAsync<TestObject>("test.json", cancellationToken: cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SerializeJsonAsync_WhenCalled_SerializesObject()
    {
        // Arrange
        IWriteOnlyAssetsService writeOnlyService = Substitute.For<IWriteOnlyAssetsService>();
        var buffer = new byte[100];
        var stream = new MemoryStream(buffer);
        writeOnlyService.GetWriteOnlySteam(Arg.Any<string>()).Returns(stream);
        var testObject = new TestObject { Name = "Test" };
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await writeOnlyService.SerializeJsonAsync("test.json", testObject, cancellationToken: cancellationToken);

        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        TestObject? resultObject = await JsonSerializer.DeserializeAsync<TestObject>(memoryStream, cancellationToken: cancellationToken);

        // Assert
        resultObject.Should().NotBeNull();
        resultObject?.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CopyToAsync_WhenCalled_CopiesStreamCorrectly()
    {
        // Arrange
        IReadOnlyAssetsService sourceService = Substitute.For<IReadOnlyAssetsService>();
        IWriteOnlyAssetsService targetService = Substitute.For<IWriteOnlyAssetsService>();
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
        var content = await reader.ReadToEndAsync();
        content.Should().Be("Test data");
    }

    private sealed class TestObject
    {
        public required string Name { get; init; }
    }
}
