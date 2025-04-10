#if ENABLE_WEBREQUEST
#nullable enable

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Collections;

namespace AndanteTribe.Utils.WebRequest
{
    /// <summary>
    /// <see cref="NativeArray{T}"/>を基盤とする<see cref="HttpContent"/>.
    /// </summary>
    public sealed class NativeArrayContent : HttpContent
    {
        private readonly NativeArray<byte>.ReadOnly _data;
        private readonly IDisposable _handler;

        public NativeArrayContent(NativeArray<byte>.ReadOnly data, IDisposable handler)
        {
            _data = data;
            _handler = handler;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext _)
        {
            stream.Write(_data.AsReadOnlySpan());
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _data.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handler.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
#endif