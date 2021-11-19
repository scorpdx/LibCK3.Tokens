using MessagePack;
using Microsoft.Extensions.FileProviders;

namespace Meltwater
{
    internal static class RawTokenProvider
    {
        private static readonly string TokenFilePath = Path.Join("Assets", "1.4.4.tok.msgpack");

        internal static Dictionary<string, ushort> ReadEmbeddedTokens()
        {
            var provider = new EmbeddedFileProvider(typeof(RawTokenProvider).Assembly);
            var tokenFile = provider.GetFileInfo(TokenFilePath);
            using var fs = tokenFile.CreateReadStream();

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Deserialize<Dictionary<string, ushort>>(fs, lz4Options);
        }
    }
}
