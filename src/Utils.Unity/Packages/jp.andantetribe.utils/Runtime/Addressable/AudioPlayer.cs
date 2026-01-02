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
    public partial class AudioPlayer : IDisposable
    {
        private readonly AudioSource[] _bgmChannels;

        private readonly AudioSource _seChannel;
        private readonly AssetsRegistry _bgmRegistry = new AssetsRegistry();
        private ReadOnlySpan<AudioSource> BgmChannels => _bgmChannels;

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
            _seChannel = root.AddComponent<AudioSource>();
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
            cancellationToken.ThrowIfCancellationRequested();
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
            cancellationToken.ThrowIfCancellationRequested();
            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            try
            {
                var result = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
                if (result == null)
                {
                    Debug.LogError($"Failed to load SE: {address}");
                    return;
                }

                _seChannel.PlayOneShot(result);

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
            _seChannel.Stop();
            _seChannel.clip = null;
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