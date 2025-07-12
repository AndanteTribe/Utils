#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO
#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AndanteTribe.Utils.Addressable
{
    /// <summary>
    /// 汎用オーディオ再生クラス.
    /// </summary>
    public partial class AudioPlayer : IDisposable
    {
        private readonly AudioSource[] _bgmChannels;

        protected readonly AudioSource SeChannel;
        protected readonly AssetsRegistry BgmRegistry;
        protected readonly CancellationTokenSource CancellationDisposable = new();
        protected ReadOnlySpan<AudioSource> BgmChannels => _bgmChannels;

        private int _currentBgmChannelIndex = -1;

        /// <summary>
        /// Initialize a new instance of <see cref="AudioPlayer"/>.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bgmChannelCount"></param>
        public AudioPlayer(GameObject root, int bgmChannelCount = 3)
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
            BgmRegistry = new AssetsRegistry(CancellationDisposable);

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
            CancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationDisposable.CreateLinkedTokenSource(cancellationToken);
            var clip = await BgmRegistry.LoadAsyncInternal<AudioClip>(address, cts.Token);
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
            CancellationDisposable.ThrowIfDisposed(this);
            foreach (var channel in BgmChannels)
            {
                if (channel.isPlaying)
                {
                    channel.Stop();
                    channel.clip = null;
                }
            }
            BgmRegistry.Clear();
            _currentBgmChannelIndex = -1;
        }

        /// <summary>
        /// SEを非同期でロードして再生する.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PlaySEAsync(string address, CancellationToken cancellationToken = default)
        {
            CancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationDisposable.CreateLinkedTokenSource(cancellationToken);
            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            try
            {
                var result = await handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true);
                if (result == null)
                {
                    Debug.LogError($"Failed to load SE: {address}");
                    return;
                }

                SeChannel.PlayOneShot(result);

                await UniTask.Delay(TimeSpan.FromSeconds(result.length), cancellationToken: cts.Token);
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
            BgmRegistry.Dispose();
        }

        protected AudioSource GetAvailableBgmChannel() => _bgmChannels[(_currentBgmChannelIndex + 1) % _bgmChannels.Length];

        partial void Initialize();
        partial void SetBgmVolume(AudioSource channel, float rate = 1.0f);
    }
}

#endif