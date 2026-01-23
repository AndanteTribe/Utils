#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO
#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AndanteTribe.Utils.Unity.Addressable
{
    /// <summary>
    /// 汎用オーディオ再生クラス.
    /// </summary>
    public partial class AudioPlayerCore : IDisposable
    {
        private readonly AudioSource[] _bgmChannels;

        protected readonly AudioSource SeChannel;
        protected readonly AssetsRegistry _bgmRegistry;
        protected ReadOnlySpan<AudioSource> BgmChannels => _bgmChannels;

        protected int _currentBgmChannelIndex = -1;

        /// <summary>
        /// Initialize a new instance of <see cref="AudioPlayerCore"/>.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bgmChannelCount"></param>
        /// <param name="bgmRegistry"></param>
        public AudioPlayerCore(GameObject root, int bgmChannelCount = 3, AssetsRegistry? bgmRegistry = null)
        {
            _bgmChannels = new AudioSource[bgmChannelCount];
            var bgmChannels = _bgmChannels.AsSpan();
            for (var i = 0; i < bgmChannels.Length; i++)
            {
                var channel = root.AddComponent<AudioSource>();
                channel.playOnAwake = false;
                channel.loop = true;
                bgmChannels[i] = channel;
            }
            SeChannel = root.AddComponent<AudioSource>();
            _bgmRegistry = bgmRegistry ?? new AssetsRegistry();
            Initialize();
        }

        /// <summary>
        /// BGMを非同期でロードして再生する.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="loop"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PlayBGMAsync(string address, bool loop = true, CancellationToken cancellationToken = default)
        {
            var clip = await _bgmRegistry.LoadAsync<AudioClip>(address, cancellationToken);
            var channel = GetAvailableBgmChannel();

            channel.Stop();
            SetBgmVolume(channel);
            channel.clip = clip;
            channel.loop = loop;
            channel.Play();
        }

        /// <summary>
        /// BGMを停止する.
        /// </summary>
        public void StopAllBGM()
        {
            foreach (var channel in BgmChannels)
            {
                if (channel.isPlaying)
                {
                    channel.Stop();
                    channel.clip = null;
                }
            }
            _bgmRegistry.Clear();
            _currentBgmChannelIndex = -1;
        }

        /// <summary>
        /// SEを非同期でロードして再生する.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PlaySEAsync(string address, CancellationToken cancellationToken = default)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            try
            {
                var result = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
                if (result == null)
                {
                    Debug.LogError($"Failed to load SE: {address}");
                    return;
                }

                SeChannel.PlayOneShot(result);

                await UniTask.Delay(TimeSpan.FromSeconds(result.length), cancellationToken: cancellationToken);
            }
            finally
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopAllBGM();
            SeChannel.Stop();
            SeChannel.clip = null;
            _bgmRegistry.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AudioSource GetAvailableBgmChannel() => _bgmChannels[(_currentBgmChannelIndex + 1) % _bgmChannels.Length];

        partial void Initialize();
        partial void Deinitialize();
        partial void SetBgmVolume(AudioSource channel, float rate = 1.0f);
    }
}

#endif