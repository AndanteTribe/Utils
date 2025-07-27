#if ENABLE_TEXTMESHPRO
#nullable enable

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils.Unity.UGUI
{
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// Sets the character array of a <see cref="TMPro.TMP_Text"/> instance using a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sourceText"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCharArray(this TMPro.TMP_Text text, in ReadOnlySpan<char> sourceText)
        {
            var array = ArrayPool<char>.Shared.Rent(sourceText.Length);
            sourceText.CopyTo(array);
            text.SetCharArray(array, 0, sourceText.Length);
            ArrayPool<char>.Shared.Return(array);
        }
    }
}

#endif