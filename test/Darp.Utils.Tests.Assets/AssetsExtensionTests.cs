namespace Darp.Utils.Tests.Assets;

using System.Text.Json;
using Common;
using NSubstitute;
using Shouldly;
using Utils.Assets;

public sealed class AssetsExtensionTests
{
    private static readonly byte[] TestConfig = /*lang=json,strict*/
    """
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
        readOnlyService.GetReadOnlyStream(Arg.Any<string>()).Returns(stream);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        TestObject result = await readOnlyService.DeserializeJsonAsync<TestObject>(
            "test.json",
            cancellationToken: cancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public async Task DeserializeJsonAsync_WhenDeserializationReturnsNull_ThrowsException()
    {
        // Arrange
        IReadOnlyAssetsService readOnlyService = Substitute.For<IReadOnlyAssetsService>();
        readOnlyService.GetReadOnlyStream(Arg.Any<string>()).Returns(new MemoryStream());
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () =>
            await readOnlyService.DeserializeJsonAsync<TestObject>("test.json", cancellationToken: cancellationToken);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task SerializeJsonAsync_WhenCalled_SerializesObject()
    {
        // Arrange
        IAssetsService writeOnlyService = Substitute.For<IAssetsService>();
        var buffer = new byte[100];
        var stream = new MemoryStream(buffer);
        writeOnlyService.GetWriteOnlySteam(Arg.Any<string>()).Returns(stream);
        var testObject = new TestObject { Name = "Test" };
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await writeOnlyService.SerializeJsonAsync("test.json", testObject, cancellationToken: cancellationToken);

        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        TestObject? resultObject = await JsonSerializer.DeserializeAsync<TestObject>(
            memoryStream,
            cancellationToken: cancellationToken
        );

        // Assert
        resultObject.ShouldNotBeNull();
        resultObject.Name.ShouldBe("Test");
    }

    [Fact]
    public async Task CopyToAsync_WhenCalled_CopiesStreamCorrectly()
    {
        // Arrange
        IReadOnlyAssetsService sourceService = Substitute.For<IReadOnlyAssetsService>();
        IAssetsService targetService = Substitute.For<IAssetsService>();
        var sourceStream = new MemoryStream("Test data"u8.ToArray());
        var buffer = new byte[100];
        var targetStream = new MemoryStream(buffer);

        sourceService.GetReadOnlyStream(Arg.Any<string>()).Returns(sourceStream);
        targetService.GetWriteOnlySteam(Arg.Any<string>()).Returns(targetStream);

        // Act
        await sourceService.CopyToAsync("source.json", targetService, "target.json");

        // Assert
        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        memoryStream.Length.ShouldBeGreaterThan(0);
        using var reader = new StreamReader(memoryStream);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe("Test data");
    }

    [Fact]
    public async Task CopyToAsyncAttributes_WhenCalled_CopiesStreamCorrectly()
    {
        // Arrange
        IReadOnlyAssetsService sourceService = Substitute.For<IReadOnlyAssetsService>();
        IFolderAssetsService targetService = Substitute.For<IFolderAssetsService>();
        var sourceStream = new MemoryStream("Test data"u8.ToArray());
        var buffer = new byte[100];
        var targetStream = new MemoryStream(buffer);

        sourceService.GetReadOnlyStream(Arg.Any<string>()).Returns(sourceStream);
        targetService.GetWriteOnlySteam(Arg.Any<string>(), Arg.Any<FileAttributes>()).Returns(targetStream);

        // Act
        await sourceService.CopyToAsync("source.json", targetService, "target.json", FileAttributes.ReadOnly);

        // Assert
        var memoryStream = new MemoryStream(buffer.TrimBufferEnd());
        memoryStream.Length.ShouldBeGreaterThan(0);
        using var reader = new StreamReader(memoryStream);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe("Test data");
    }

    private sealed class TestObject
    {
        public required string Name { get; init; }
    }
}
