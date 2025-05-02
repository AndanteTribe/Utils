#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO
#nullable enable

using System;
using System.Threading;
using AndanteTribe.Utils.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils.Addressables
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// 汎用オーディオ再生クラス.
    /// </summary>
    public partial class AudioPlayer : ITrackableDisposable
    {
        private readonly AudioSource[] _bgmChannels;

        protected readonly AudioSource SeChannel;
        protected readonly AssetsRegistry BgmRegistry;
        protected ReadOnlySpan<AudioSource> BgmChannels => _bgmChannels;

        public bool IsDisposed => BgmRegistry.IsDisposed;

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
            BgmRegistry = new AssetsRegistry();

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
            var clip = await BgmRegistry.LoadAsync<AudioClip>(address, cancellationToken);
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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
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
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, BgmRegistry.DisposableToken);
            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            var result = await handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true);
            if (result == null)
            {
                Debug.LogError($"Failed to load SE: {address}");
                return;
            }

            SeChannel.PlayOneShot(result);

            await UniTask.Delay(TimeSpan.FromSeconds(result.length), cancellationToken: cts.Token);
            if (handle.IsValid())
            {
                handle.Release();
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