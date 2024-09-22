namespace Darp.Utils.Tests.Common;

public static class TestExtensions
{
    public static byte[] TrimBufferEnd(this byte[] buffer)
    {
        var i = buffer.Length - 1;
        for (; i >= 0; i--)
        {
            if (buffer[i] is not 0x0)
            {
                break;
            }
        }
        return buffer[..(i + 1)];
    }
}
