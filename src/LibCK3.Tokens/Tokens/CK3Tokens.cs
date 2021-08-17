using System.Collections.Generic;
using System.Text.Json;
namespace LibCK3.Tokens
{
    public static partial class CK3Tokens
    {
        /// <summary>
        /// 1.4.4 2021-06-29 79ad Hotfix 
        /// </summary>
        public static IReadOnlyDictionary<ushort, JsonEncodedText> Tokens { get; }

        /// <summary>
        /// 1.4.4 2021-06-29 79ad Hotfix 
        /// </summary>
        public static IReadOnlyDictionary<string, ushort> TokenNames { get; }
    }
}
