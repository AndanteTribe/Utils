#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO && ENABLE_R3
#nullable enable

using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace AndanteTribe.Utils.Addressables
{
    public partial class AudioPlayer
    {
        /// <summary>
        /// マスターボリューム.
        /// </summary>
        public readonly ReactiveProperty<float> MasterVolume = new ReactiveProperty<float>(0.5f);

        /// <summary>
        /// BGMボリューム.
        /// </summary>
        public readonly ReactiveProperty<float> BgmVolume = new ReactiveProperty<float>(0.5f);

        /// <summary>
        /// SEボリューム.
        /// </summary>
        public readonly ReactiveProperty<float> SeVolume = new ReactiveProperty<float>(0.5f);

        partial void Initialize()
        {
            BgmVolume
                .CombineLatest(MasterVolume, static (bgmVolume, masterVolume) => bgmVolume * masterVolume)
                .Subscribe(_bgmChannels, static (value, channels) =>
                {
                    foreach (var channel in channels.AsSpan())
                    {
                        channel.volume = value;
                    }
                })
                .AddTo(BgmRegistry.DisposableToken);
            SeVolume
                .CombineLatest(MasterVolume, static (seVolume, masterVolume) => seVolume * masterVolume)
                .Subscribe(SeChannel, static (value, channel) => channel.volume = value)
                .AddTo(BgmRegistry.DisposableToken);
        }

        partial void SetBgmVolume(AudioSource channel, float rate)
            => channel.volume = BgmVolume.Value * rate * MasterVolume.Value;
    }
}

#endif