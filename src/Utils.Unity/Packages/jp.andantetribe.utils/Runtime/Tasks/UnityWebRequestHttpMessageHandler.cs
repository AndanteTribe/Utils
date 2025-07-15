#if ENABLE_WEBREQUEST && ENABLE_UNITASK
#nullable enable

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace AndanteTribe.Utils.Tasks
{
    /// <summary>
    /// UniTaskのサポートを活用した、<see cref="UnityWebRequest"/>を使用する<see cref="HttpMessageHandler"/>.
    /// </summary>
    /// <exception cref="UnityWebRequestException">通信結果のエラーはUniTask製のExceptionクラスで発行される.</exception>
    public sealed class UnityWebRequestHttpMessageHandler : HttpMessageHandler
    {
        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var data = requestMessage.Content == null ? null : await requestMessage.Content.ReadAsByteArrayAsync();
            var uploadHandler = new UploadHandlerRaw(data);
            uploadHandler.contentType = requestMessage.Content?.Headers?.ContentType?.ToString() ?? "";

            var downloadHandler = new DownloadHandlerBuffer();
            var webRequest = new UnityWebRequest(requestMessage.RequestUri, requestMessage.Method.ToString(), downloadHandler, uploadHandler);

            foreach (var (key, values) in requestMessage.Headers)
            {
                webRequest.SetRequestHeader(key, string.Join(',', values));
            }

            await webRequest.SendWebRequest().WithCancellation(cancellationToken);

            var response = new HttpResponseMessage((HttpStatusCode)webRequest.responseCode)
            {
                RequestMessage = requestMessage,
                Content = new ByteArrayContent(downloadHandler.data),
                Version = HttpVersion.Version10
            };

            var responseHeader = response.Headers;
            foreach (var (key, value) in webRequest.GetResponseHeaders())
            {
                responseHeader.TryAddWithoutValidation(key, value);
            }

            return response;
        }
    }
}
#endif
