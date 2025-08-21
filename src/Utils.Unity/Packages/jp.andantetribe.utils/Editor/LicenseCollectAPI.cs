#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;

namespace AndanteTribe.Utils.Unity.Editor
{
    /// <summary>
    /// ライセンス情報のレスポンス.
    /// </summary>
    /// <param name="Name">ライブラリ名.</param>
    /// <param name="Url">ライセンスのURL.</param>
    /// <param name="Content">ライセンスの内容.</param>
    public sealed record LicenseResponse(string Name, string Url, string Content);

    public class LicenseCollectAPI : IDisposable
    {
        public readonly HttpClient HttpClient;

        public readonly uint RetryCount;

        public LicenseCollectAPI(HttpMessageHandler handler, in uint retryCount, in TimeSpan timeout)
        {
            HttpClient = new HttpClient(handler, true) { Timeout = timeout };
            RetryCount = retryCount;
        }

        [MenuItem("Tools/AndanteTribe/Collect Licenses")]
        public static void Initialize()
        {
            var client = new LicenseCollectAPI(new HttpClientHandler(), 3, TimeSpan.FromSeconds(30));
            _ = client.CollectAsync();
        }

        public async Task<LicenseResponse[]> CollectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Unity Package Managerからパッケージ情報を取得
            var listRequest = Client.List(false);
            while (!listRequest.IsCompleted)
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
            if (listRequest.Status != StatusCode.Success)
            {
                throw new Exception($"Failed to collect licenses: {listRequest.Error.message}");
            }

            // 得られた結果からGitHub経由パッケージを抽出
            // ライセンスを取得するためのAPIのURLを生成
            var licenseAPIs = listRequest.Result
                .Where(static package => package.source == PackageSource.Git)
                .Select(static package => new Uri(GetAfterAt(package.packageId, '@').ToString()))
                .Where(static repoUrl => repoUrl.Host.Contains("github.com"))
                .Select(static repoUrl =>
                {
                    var sb = new StringBuilder();
                    sb.Append("https://api.github.com/repos/");
                    sb.Append(repoUrl.Segments[1]); // owner(e.g., "AndanteTribe/")
                    sb.Append(repoUrl.Segments[2]); // repo(e.g., "Utils.git/")
                    sb.Remove(sb.Length - 5, 5); // Remove ".git/"
                    sb.Append("/license");
                    return new Uri(sb.ToString());
                });

            // 各ライセンスAPIからライセンス情報を取得
            // HttpClientを使用して非同期でリクエストを送信


            return Array.Empty<LicenseResponse>();
        }

        public void Dispose()
        {

        }

        // private static Uri GetLicenseApiUrl(Uri repositoryUrl)
        // {
        //     var owner = repositoryUrl.Segments[1];
        //     var repo = GetBeforeAt(repositoryUrl.Segments[2], stackalloc[]{ '.', 'g', 'i', 't' });
        //     var sb = new DefaultInterpolatedStringHandler(64, 2);
        //     sb.AppendLiteral("https://api.github.com/repos/");
        //     sb.AppendFormatted(owner);
        //     sb.AppendFormatted(repo);
        //     sb.AppendLiteral("/license");
        //     return new Uri(sb.ToStringAndClear());
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetBeforeAt(in ReadOnlySpan<char> span, in ReadOnlySpan<char> c)
        {
            var index = span.IndexOf(c);
            return index < 0 ? ReadOnlySpan<char>.Empty : span[..index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetAfterAt(in ReadOnlySpan<char> span, char c)
        {
            var index = span.IndexOf(c);
            return index < 0 ? ReadOnlySpan<char>.Empty : span[(index + 1)..];
        }
    }
}