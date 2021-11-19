using Microsoft.CodeAnalysis;
using System.Collections.ObjectModel;

namespace Meltwater
{
    [Generator]
    public class CK3TokensGenerator : ISourceGenerator
    {
        private Dictionary<string, ushort> _rawTokens;

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