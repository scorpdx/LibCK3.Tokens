using Microsoft.CodeAnalysis;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;

namespace Meltwater;

[Generator]
public class CK3TokensGenerator : ISourceGenerator
{
    const string @namespace = "LibCK3.Tokens";
    const string @class = "CK3Tokens";

    private Dictionary<ushort, string> _rawTokens;

    private static IEnumerable<string> ToRakalyFormat(IReadOnlyDictionary<ushort, string> tokenDict)
        => tokenDict.Select(kvp => string.Format("0x{0:X4} {1}", kvp.Key, kvp.Value));

    private static IEnumerable<string> ToLibCK3Format(IReadOnlyDictionary<ushort, string> tokenDict)
    {
//#if DEBUG
//        if(!Debugger.IsAttached) Debugger.Launch();
//#endif
        yield return "using System.Collections.Generic;";
        yield return "using System.Collections.ObjectModel;";
        yield return "using System.Text.Json;";
        yield return $"namespace {@namespace};";
        yield return $"public static partial class {@class}";
        yield return "{";
        yield return $"    private static ReadOnlyDictionary<ushort, JsonEncodedText> _tokens = new(new Dictionary<ushort, JsonEncodedText>";
        yield return "    {";
        foreach (var kvp in tokenDict)
        {
            yield return $"        {{ 0x{kvp.Key:X4}, JsonEncodedText.Encode(\"{kvp.Value}\") }},";
        }
        yield return "    });";
        yield return "}";
    }

    private static IEnumerable<string> ToLibCK3NameFormat(IReadOnlyDictionary<ushort, string> tokenDict)
    {
        yield return "using System.Collections.Generic;";
        yield return "using System.Collections.ObjectModel;";
        yield return "using System.Text.Json;";
        yield return $"namespace {@namespace};";
        yield return $"public static partial class {@class}";
        yield return "{";
        yield return $"    private static ReadOnlyDictionary<string, ushort> _tokenNames = new(new Dictionary<string, ushort>";
        yield return "    {";
        foreach (var kvp in tokenDict)
        {
            yield return $"        {{ \"{kvp.Value}\", 0x{kvp.Key:X4} }},";
        }
        yield return "    });";
        yield return "}";
    }

    //private static string san(string text) => char.IsDigit(text = text.Replace("_", "__").Replace(':', '_').Replace('.', '_')
    //                                                                  .Replace('&', '_').Replace('}', '_')
    //                                                                  .Replace('-', '_'), 0) ? $"v{text}" : text;

    //private static IEnumerable<string> ToLibCK3TokenDefinitions(IEnumerable<(string version, ushort id, string token)> allTokens)
    //{
    //    yield return "using System.Collections.Generic;";
    //    yield return "using System.Text.Json;";
    //    yield return "namespace LibCK3.Tokens";
    //    yield return "{";
    //    yield return "    public static partial class CK3Tokens";
    //    yield return "    {";
    //    foreach (var token in allTokens.Select(a => a.token).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct())
    //    {
    //        yield return $"        private static JsonEncodedText? @{san(token)};";
    //    }
    //    yield return "    }";
    //    yield return "}";
    //}

    private static IEnumerable<string> ToLibCK3TokenSwitch(IReadOnlyDictionary<ushort, string> tokens)
    {
        yield return "using System;";
        yield return "using System.Collections.Generic;";
        yield return "using System.Text.Json;";
        yield return $"namespace {@namespace};";
        yield return $"public static partial class {@class}";
        yield return "{";
        yield return $"    private static string ParseToken(ushort ck3id)";
        yield return "      => ck3id switch";
        yield return "      {";
        foreach (var a in tokens.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            yield return $"          {a.Key} => \"{a.Value}\",";
        }
        yield return @"          _ => throw new InvalidOperationException(""Not a valid token identifier"")";
        yield return "      };";
        yield return "}";
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource("CK3Tokens.Json.cs", string.Join(Environment.NewLine, ToLibCK3Format(_rawTokens)));
        context.AddSource("CK3Tokens.Names.cs", string.Join(Environment.NewLine, ToLibCK3NameFormat(_rawTokens)));
        context.AddSource("CK3Tokens.Switch.cs", string.Join(Environment.NewLine, ToLibCK3TokenSwitch(_rawTokens)));
    }
    public void Initialize(GeneratorInitializationContext context)
    {
        _rawTokens = RawTokenProvider.ReadEmbeddedTokens();
    }
}