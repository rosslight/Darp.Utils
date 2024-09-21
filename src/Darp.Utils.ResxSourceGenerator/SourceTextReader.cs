namespace Darp.Utils.ResxSourceGenerator;

using Microsoft.CodeAnalysis.Text;

internal sealed class SourceTextReader(SourceText text) : TextReader
{
    private readonly SourceText _text = text;
    private int _position;

    public override int Read(char[] buffer, int index, int count)
    {
        var remaining = _text.Length - _position;
        var charactersToRead = Math.Min(remaining, count);
        _text.CopyTo(_position, buffer, index, charactersToRead);
        _position += charactersToRead;
        return charactersToRead;
    }
}
