﻿using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Meltwater;

[Generator]
public class CK3TokensGenerator : ISourceGenerator
{
    const string @namespace = "LibCK3.Tokens";
    const string @class = "CK3Tokens";

    private Dictionary<ushort, string> _rawTokens;

    private static IEnumerable<string> ToRakalyFormat(IReadOnlyDictionary<ushort, string> tokenDict)
        => tokenDict.Select(kvp => $"0x{kvp.Key:X4} {kvp.Value}");

    private static string ToLibCK3Format(IReadOnlyDictionary<ushort, string> tokenDict)
    {
        var sb = new StringBuilder();
        a("using System.Collections.Generic;");
        a("using System.Collections.ObjectModel;");
        a("using System.Text.Json;");
        a($"namespace {@namespace};");
        a($"public static partial class {@class}");
        a("{");
        a($"    private static ReadOnlyDictionary<ushort, JsonEncodedText> _tokens = new(new Dictionary<ushort, JsonEncodedText>");
        a("    {");
        foreach (var kvp in tokenDict)
        {
            a($"        {{ 0x{kvp.Key:X4}, JsonEncodedText.Encode(\"{kvp.Value}\") }},");
        }
        a("    });");
        a("}");

        return sb.ToString();
        StringBuilder a(string s) => sb.AppendLine(s);
    }

    private static string ToLibCK3NameFormat(IReadOnlyDictionary<ushort, string> tokenDict)
    {
        var sb = new StringBuilder();
        a("using System.Collections.Generic;");
        a("using System.Collections.ObjectModel;");
        a($"namespace {@namespace};");
        a($"public static partial class {@class}");
        a("{");
        a($"    private static ReadOnlyDictionary<string, ushort> _tokenNames = new(new Dictionary<string, ushort>");
        a("    {");
        foreach (var kvp in tokenDict)
        {
            a($"        {{ \"{kvp.Value}\", 0x{kvp.Key:X4} }},");
        }
        a("    });");
        a("}");

        return sb.ToString();
        StringBuilder a(string s) => sb.AppendLine(s);
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
        context.AddSource("CK3Tokens.Json.cs", ToLibCK3Format(_rawTokens));
        context.AddSource("CK3Tokens.Names.cs", ToLibCK3NameFormat(_rawTokens));
        //context.AddSource("CK3Tokens.Switch.cs", string.Join(Environment.NewLine, ToLibCK3TokenSwitch(_rawTokens)));
    }
    public void Initialize(GeneratorInitializationContext context)
    {
        _rawTokens = RawTokenProvider.ReadEmbeddedTokens();
        CheckTokenNames(_rawTokens.Values);
    }

    [Conditional("DEBUG")]
    public void CheckTokenNames(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var utf8name = Encoding.UTF8.GetBytes(name);

            int idx;
            if ((idx = NeedsEscaping(utf8name)) != -1)
            {
                Debug.WriteLine($"{name} needs escape at idx {idx}");
            }
        }

        static int NeedsEscaping(ReadOnlySpan<byte> value) => JavaScriptEncoder.Default.FindFirstCharacterToEncodeUtf8(value);
    }
}