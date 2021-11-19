using Microsoft.CodeAnalysis;
using System.Collections.ObjectModel;

namespace Meltwater
{
    [Generator]
    public class CK3TokensGenerator : ISourceGenerator
    {
        private Dictionary<string, ushort> _rawTokens;
        private static Dictionary<ushort, string> ParseTokens(IEnumerable<string> lines)
            => lines.Select(line => line.Split(' '))
            .Where(a => a.Length == 2)
            .Select(a => new
            {
                ID = ushort.Parse(a[1]),
                Name = a[0]
            })
            //.Where(a => a.ID >= Offset)
            .ToDictionary(a => /*checked((ushort)(a.ID - Offset))*/a.ID, a => a.Name);

        private static IEnumerable<string> ToRakalyFormat(IReadOnlyDictionary<ushort, string> tokenDict)
            => tokenDict.Select(kvp => string.Format("0x{0:X4} {1}", kvp.Key, kvp.Value));

        private static IEnumerable<string> ToLibCK3Format(string version, IReadOnlyDictionary<ushort, string> tokenDict)
        {
            yield return "using System.Collections.Generic;";
            yield return "using System.Text.Json;";
            yield return "namespace LibCK3.Tokens";
            yield return "{";
            yield return "    public static partial class CK3Tokens";
            yield return "    {";
            yield return $"        private static IReadOnlyDictionary<ushort, JsonEncodedText> Tokens{version.Replace('.', '_')} {{ get; }} = new Dictionary<ushort, JsonEncodedText>";
            yield return "        {";
            foreach (var kvp in tokenDict)
            {
                yield return $"            {{0x{kvp.Key:X4}, JsonEncodedText.Encode(\"{kvp.Value}\")}},";
            }
            yield return "        };";
            yield return "    }";
            yield return "}";
        }

        private static IEnumerable<string> ToLibCK3NameFormat(string version, IReadOnlyDictionary<ushort, string> tokenDict)
        {
            yield return "using System.Collections.Generic;";
            yield return "using System.Text.Json;";
            yield return "namespace LibCK3.Tokens";
            yield return "{";
            yield return "    public static partial class CK3Tokens";
            yield return "    {";
            yield return $"        private static IReadOnlyDictionary<ushort, JsonEncodedText> Tokens{version.Replace('.', '_')} {{ get; }} = new Dictionary<string, ushort>";
            yield return "        {";
            foreach (var kvp in tokenDict)
            {
                yield return $"            {{\"{kvp.Value}\", 0x{kvp.Key:X4}}},";
            }
            yield return "        };";
            yield return "    }";
            yield return "}";
        }

        private static string san(string text) => char.IsDigit(text = text.Replace("_", "__").Replace(':', '_').Replace('.', '_')
                                                                          .Replace('&', '_').Replace('}', '_')
                                                                          .Replace('-', '_'), 0) ? $"v{text}" : text;

        private static IEnumerable<string> ToLibCK3TokenDefinitions(IEnumerable<(string version, ushort id, string token)> allTokens)
        {
            yield return "using System.Collections.Generic;";
            yield return "using System.Text.Json;";
            yield return "namespace LibCK3.Tokens";
            yield return "{";
            yield return "    public static partial class CK3Tokens";
            yield return "    {";
            foreach (var token in allTokens.Select(a => a.token).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct())
            {
                yield return $"        private static JsonEncodedText? @{san(token)};";
            }
            yield return "    }";
            yield return "}";
        }

        private static IEnumerable<string> ToLibCK3TokenTable(string version, IReadOnlyDictionary<ushort, string> tokens)
        {
            const string throwFragment = @"throw new InvalidOperationException(""Token identifier is null"")";

            yield return "using System;";
            yield return "using System.Collections.Generic;";
            yield return "using System.Text.Json;";
            yield return "namespace LibCK3.Tokens";
            yield return "{";
            yield return "    public static partial class CK3Tokens";
            yield return "    {";
            yield return $"        private static JsonEncodedText ParseToken_{san(version)}(ushort ck3id)";
            yield return "          => ck3id switch";
            yield return "          {";
            foreach (var a in tokens)
            {
                yield return string.IsNullOrWhiteSpace(a.Value)
                    ? $"              {a.Key} => {throwFragment},"
                    : $"              {a.Key} => JsonEncodedText.Encode(\"{a.Value}\"),";
            }
            yield return @"              _ => throw new InvalidOperationException(""No tokens matched"")";
            yield return "          };";
            yield return "    }";
            yield return "}";
        }

        public void Execute(GeneratorExecutionContext context)
        {
            const string @namespace = "LibCK3.Tokens";
            const string @class = "CK3Tokens";

            // build up the source code
            string source = $@"
using System;

namespace {@namespace}
{{
    public static partial class {@class}
    {{
        public static ReadOnlyDictionary<ushort, JsonEncodedText> Tokens {{ get; }} = new(new Dictionary<ushort, JsonEncodedText> {{
            {{ }}
        }}
    }}
}}
";
            // add the source code to the compilation
            context.AddSource("CK3Tokens.Strings.cs", source);
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            _rawTokens = RawTokenProvider.ReadEmbeddedTokens();
        }
    }
}