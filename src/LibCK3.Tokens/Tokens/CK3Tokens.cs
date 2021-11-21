using System.Collections.ObjectModel;
using System.Text.Json;
namespace LibCK3.Tokens
{
    public static partial class CK3Tokens
    {
        /// <summary>
        /// 1.4.4 2021-06-29 79ad Hotfix 
        /// </summary>
        public static ReadOnlyDictionary<ushort, JsonEncodedText> Tokens => _tokens;
    }
}
