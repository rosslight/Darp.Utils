namespace Darp.Utils.Tests.Assets;

using System.Globalization;
using System.Text;
using Shouldly;
using Utils.Assets;

public class MemoryAssetsServiceTests
{
    private const string BasePath = "base/path";
    private readonly MemoryAssetsService _service = new(BasePath);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Exists_WithNullOrWhiteSpacePath_ReturnsFalse(string? path)
    {
        _service.Exists(path).ShouldBeFalse();
    }

    [Fact]
    public void GetReadOnlySteam_MissingFile_ThrowsFileNotFoundException()
    {
        Should.Throw<FileNotFoundException>(() => _service.GetReadOnlyStream("nonexistent.txt"));
    }

    [Fact]
    public void WriteAndRead_File_PersistsData()
    {
        const string path = "foo.txt";
        const string content = "Hello, world!";
        var bytes = Encoding.UTF8.GetBytes(content);

        // Write
        using (Stream writeStream = _service.GetWriteOnlySteam(path))
        {
            writeStream.Write(bytes, 0, bytes.Length);
        }

        // Verify Exists
        _service.Exists(path).ShouldBeTrue();
        _service.Exists(path.ToUpper(CultureInfo.InvariantCulture)).ShouldBeFalse(); // case-sensitive

        // Read
        using (Stream readStream = _service.GetReadOnlyStream(path))
        using (var reader = new StreamReader(readStream, Encoding.UTF8))
        {
            var readContent = reader.ReadToEnd();
            readContent.ShouldBe(content);
        }
    }

    [Fact]
    public void Overwrite_File_UpdatesData()
    {
        const string path = "test.bin";
        var initial = new byte[] { 1, 2, 3 };
        var updated = new byte[] { 4, 5, 6, 7 };

        // Initial write
        using (Stream w1 = _service.GetWriteOnlySteam(path))
            w1.Write(initial, 0, initial.Length);

        // Overwrite
        using (Stream w2 = _service.GetWriteOnlySteam(path))
            w2.Write(updated, 0, updated.Length);

        // Read back
        using (Stream r = _service.GetReadOnlyStream(path))
        {
            var buffer = new byte[updated.Length];
            var read = r.Read(buffer);
            read.ShouldBe(updated.Length);
            buffer.ShouldBe(updated);
        }
    }

    [Fact]
    public void EnumerateFiles_PatternMatching_Works()
    {
        // Arrange multiple files
        var files = new[] { "a.txt", "b.cs", "c.txt", "sub/folder.doc" };
        foreach (var f in files)
        {
            using Stream ws = _service.GetWriteOnlySteam(f);
            ws.WriteByte(0);
        }

        // Match all text files (case-insensitive)
        var txtFiles = _service.EnumerateFiles("*.txt").ToArray();
        txtFiles.Length.ShouldBe(2);
        txtFiles.ShouldContain("a.txt");
        txtFiles.ShouldContain("c.txt");

        // Match all files
        _service.EnumerateFiles().Count().ShouldBe(files.Length);
    }

    [Fact]
    public void ReadOnlySteam_IsNotWritable()
    {
        const string path = "readonly.txt";
        var bytes = "data"u8.ToArray();
        using (Stream ws = _service.GetWriteOnlySteam(path))
            ws.Write(bytes, 0, bytes.Length);

        // Attempt to write to read-only
        using Stream rs = _service.GetReadOnlyStream(path);
        Should.Throw<NotSupportedException>(() => rs.WriteByte(0));
    }

    [Fact]
    public async Task SerializeJsonAsync_DeserializeJsonAsync_ShouldEqual()
    {
        const string path = "path/to/file.json";
        var data = new TestData("Some Value");

        await _service.SerializeJsonAsync(path, data);
        TestData deserialized = await _service.DeserializeJsonAsync<TestData>(path);

        deserialized.ShouldBe(data);
    }
}

file sealed record TestData(string Prop1);
