#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO
#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
#if ENABLE_R3
using R3;
#endif
using UnityEngine;

namespace AndanteTribe.Utils.Addressables
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// 汎用オーディオ再生クラス.
    /// </summary>
    public class AudioPlayer
    {
        protected const int BgmChannelCount = 2;

        private readonly AudioSource[] _bgmChannels;

        protected readonly AudioSource SeChannel;
        protected readonly AssetsRegistry BgmRegistry;
        protected ReadOnlySpan<AudioSource> BgmChannels => _bgmChannels;

#if ENABLE_R3
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
#endif

        /// <summary>
        /// Initialize a new instance of <see cref="AudioPlayer"/>.
        /// </summary>
        /// <param name="root"></param>
        public AudioPlayer(GameObject root)
        {
            var bgmChannels = new AudioSource[BgmChannelCount];
            for (var i = 0; i < BgmChannelCount; i++)
            {
                var channel = root.AddComponent<AudioSource>();
                channel.playOnAwake = false;
                channel.loop = true;
                bgmChannels[i] = channel;
            }
            _bgmChannels = bgmChannels;
            SeChannel = root.AddComponent<AudioSource>();
            BgmRegistry = new AssetsRegistry();

#if ENABLE_R3
            BgmVolume
                .CombineLatest(MasterVolume, static (bgmVolume, masterVolume) => bgmVolume * masterVolume)
                .Subscribe(bgmChannels, static (value, channels) =>
                {
                    foreach (var channel in channels)
                    {
                        channel.volume = value;
                    }
                })
                .AddTo(root);
            SeVolume
                .CombineLatest(MasterVolume, static (seVolume, masterVolume) => seVolume * masterVolume)
                .Subscribe(SeChannel, static (value, channel) => channel.volume = value)
                .AddTo(root);
#endif
        }

        /// <summary>
        /// BGMを非同期でロードして再生する.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="loop"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PlayBGMAsync(string address, bool loop = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var clip = await BgmRegistry.LoadAsync<AudioClip>(address, cancellationToken);

            var channel = Array.Find(_bgmChannels, static c => !c.isPlaying);
            channel ??= _bgmChannels[0];

            channel.clip = clip;
            channel.loop = loop;
            channel.Play();
        }

        /// <summary>
        /// BGMを停止する.
        /// </summary>
        public void StopAllBGM()
        {
            foreach (var channel in _bgmChannels.AsSpan())
            {
                if (channel.isPlaying)
                {
                    channel.Stop();
                    channel.clip = null;
                }
            }
            BgmRegistry.Clear();
        }

        /// <summary>
        /// SEを非同期でロードして再生する.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PlaySEAsync(string address, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            var result = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            if (result == null)
            {
                Debug.LogError($"Failed to load SE: {address}");
                return;
            }

            SeChannel.PlayOneShot(result);

            await UniTask.Delay(TimeSpan.FromSeconds(result.length), cancellationToken: cancellationToken);
            if (handle.IsValid())
            {
                handle.Release();
            }
        }
    }
}

#endif