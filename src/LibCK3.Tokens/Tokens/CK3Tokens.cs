using MessagePack;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace LibCK3.Tokens
{
    public static partial class CK3Tokens
    {
        private static readonly string TokenFilePath = Path.Join("Assets", "1.4.4.tok.msgpack");

        private static readonly EmbeddedFileProvider _provider = new(typeof(CK3Tokens).Assembly);
        private static Dictionary<string, ushort> ReadEmbeddedTokens()
        {
            var tokenFile = _provider.GetFileInfo(TokenFilePath);
            using var fs = tokenFile.CreateReadStream();

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Deserialize<Dictionary<string, ushort>>(fs, lz4Options);
        }

        private static readonly Lazy<Dictionary<ushort, JsonEncodedText>> _tokens
            = new(() => _tokenNames.Value.ToDictionary(kvp => kvp.Value, kvp => JsonEncodedText.Encode(kvp.Key)));
        /// <summary>
        /// 1.4.4 2021-06-29 79ad Hotfix 
        /// </summary>
        public static IReadOnlyDictionary<ushort, JsonEncodedText> Tokens => _tokens.Value;

        private static readonly Lazy<Dictionary<string, ushort>> _tokenNames = new(ReadEmbeddedTokens);
        /// <summary>
        /// 1.4.4 2021-06-29 79ad Hotfix 
        /// </summary>
        public static IReadOnlyDictionary<string, ushort> TokenNames => _tokenNames.Value;
    }
}
