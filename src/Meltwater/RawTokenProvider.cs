using Microsoft.Extensions.FileProviders;

namespace Meltwater;

internal static class RawTokenProvider
{
    private const string TokenFilePath = "Assets/1.4.4.tok";

    private static Dictionary<ushort, string> ParseTokens(IEnumerable<string> lines)
        => lines.Select(line => line.Split(' '))
                .Where(a => a.Length == 2)
                .Select(a => new
                {
                    ID = ushort.Parse(a[1]),
                    Name = a[0]
                })
                .ToDictionary(a => a.ID, a => a.Name);

    private static IEnumerable<string> ReadEmbeddedTokenLines()
    {
        var provider = new EmbeddedFileProvider(typeof(RawTokenProvider).Assembly);
        var tokenFile = provider.GetFileInfo(TokenFilePath);
        using var fs = tokenFile.CreateReadStream();
        using var sr = new StreamReader(fs);

        string line;
        while((line = sr.ReadLine()) != null)
        {
            yield return line;
        }
    }

    internal static Dictionary<ushort, string> ReadEmbeddedTokens() => ParseTokens(ReadEmbeddedTokenLines());
}
