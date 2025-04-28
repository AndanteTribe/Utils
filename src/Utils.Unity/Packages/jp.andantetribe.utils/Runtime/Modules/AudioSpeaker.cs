#if ENABLE_AUDIO
#nullable enable

using UnityEngine;

namespace AndanteTribe.Utils.Modules
{
    /// <summary>
    /// 汎用オーディオ再生クラス.
    /// </summary>
    public class AudioSpeaker
    {
        private readonly AudioSource _bgmChannel;
        private readonly AudioSource _seChannel;
    }
}

#endif