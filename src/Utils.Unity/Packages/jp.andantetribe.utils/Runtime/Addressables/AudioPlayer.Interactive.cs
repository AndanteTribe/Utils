#if ENABLE_ADDRESSABLES && ENABLE_UNITASK && ENABLE_AUDIO && ENABLE_LITMOTION
#nullable enable

using System;
using System.Threading;
using AndanteTribe.Utils.Internal;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace AndanteTribe.Utils.Addressables
{
    public partial class AudioPlayer
    {
        public virtual TimeSpan FadeDuration { get; init; } = TimeSpan.FromSeconds(3.0f);

        public async UniTask CrossFadeBGMAsync(string address, bool loop = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, BgmRegistry.DisposableToken);
            var clip = await BgmRegistry.LoadAsyncInternal<AudioClip>(address, cts.Token);

            // 再生曲がなければフェードインで再生開始
            if (_currentBgmChannelIndex != -1)
            {
                var channel = GetAvailableBgmChannel();
                channel.Stop();
                channel.clip = clip;
                channel.loop = loop;
                SetBgmVolume(channel, 0.0f);
                channel.Play();

                // 0.0~PI/2
                await LMotion.Create(0.0f, 1.0f, (float)FadeDuration.TotalSeconds)
                    .Bind((self: this, channel), static (rate, args) =>
                        args.self.SetBgmVolume(args.channel, Mathf.PI * 0.5f * rate))
                    .ToUniTask(cts.Token);

                return;
            }

            var currentChannel = _bgmChannels[_currentBgmChannelIndex];
            var nextChannel = GetAvailableBgmChannel();
            nextChannel.Stop();
            nextChannel.clip = clip;
            nextChannel.loop = loop;
            SetBgmVolume(nextChannel, 0.0f);
            nextChannel.time = currentChannel.time;
            nextChannel.Play();

            await LMotion.Create(0.0f, 1.0f, (float)FadeDuration.TotalSeconds)
                .Bind((self: this, cur: currentChannel, next: nextChannel), static (rate, args) =>
                {
                    // NOTE:
                    // Sin/Cosカーブでフェードすると,聴覚上の音量が常に均等になる.
                    // 直線にすると,FadeDurationの中間地点で一回音量が下がる瞬間が発生する.
                    var (self, cur, next) = args;
                    var f = Mathf.PI * 0.5f * rate;
                    self.SetBgmVolume(cur, Mathf.Cos(f));
                    self.SetBgmVolume(next, Mathf.Sin(f));
                })
                .ToUniTask(cts.Token);
        }
    }
}

#endif