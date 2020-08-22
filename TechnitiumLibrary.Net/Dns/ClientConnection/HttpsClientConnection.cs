﻿/*
Technitium Library
Copyright (C) 2020  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TechnitiumLibrary.Net.Proxy;

namespace TechnitiumLibrary.Net.Dns.ClientConnection
{
    public class HttpsClientConnection : DnsClientConnection
    {
        #region variables

        readonly HttpClient _httpClient;

        bool _pooled;
        DateTime _lastQueried;

        #endregion

        #region constructor

        public HttpsClientConnection(NameServerAddress server, NetProxy proxy)
            : base(DnsTransportProtocol.Https, server, proxy)
        {
            if (proxy == null)
            {
                _httpClient = new HttpClient();
            }
            else
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Proxy = proxy;

                _httpClient = new HttpClient(handler);
            }

            _httpClient.DefaultRequestHeaders.Add("accept", "application/dns-message");
            _httpClient.DefaultRequestHeaders.Add("host", _server.DnsOverHttpEndPoint.Host + ":" + _server.DnsOverHttpEndPoint.Port);
            _httpClient.DefaultRequestHeaders.Add("user-agent", "DoH client");
        }

        #endregion

        #region IDisposable

        bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && !_pooled)
            {
                if (_httpClient != null)
                    _httpClient.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region public

        public override async Task<DnsDatagram> QueryAsync(DnsDatagram request, int timeout, int retries, CancellationToken cancellationToken)
        {
            _lastQueried = DateTime.UtcNow;

            HttpRequestMessage httpRequest;
            {
                //serialize request
                byte[] requestBuffer;

                using (MemoryStream mS = new MemoryStream(32))
                {
                    request.WriteToUdp(mS);
                    requestBuffer = mS.ToArray();
                }

                Uri queryUri;

                if (_proxy == null)
                {
                    if (_server.IsIPEndPointStale)
                        await _server.RecursiveResolveIPAddressAsync();

                    queryUri = new Uri(_server.DnsOverHttpEndPoint.Scheme + "://" + _server.IPEndPoint.ToString() + _server.DnsOverHttpEndPoint.PathAndQuery);
                }
                else
                {
                    if (_server.IPEndPoint == null)
                        queryUri = _server.DnsOverHttpEndPoint;
                    else
                        queryUri = new Uri(_server.DnsOverHttpEndPoint.Scheme + "://" + _server.IPEndPoint.ToString() + _server.DnsOverHttpEndPoint.PathAndQuery);
                }

                httpRequest = new HttpRequestMessage(HttpMethod.Post, queryUri);
                httpRequest.Content = new ByteArrayContent(requestBuffer);
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");
            }

            //DoH wire format request
            Stopwatch stopwatch = new Stopwatch();
            int retry = 0;
            while (retry < retries) //retry loop
            {
                retry++;

                if (cancellationToken.IsCancellationRequested)
                    return await Task.FromCanceled<DnsDatagram>(cancellationToken); //task cancelled

                stopwatch.Start();

                Task<HttpResponseMessage> task = _httpClient.SendAsync(httpRequest, cancellationToken);

                using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource())
                {
                    using (CancellationTokenRegistration ctr = cancellationToken.Register(delegate () { timeoutCancellationTokenSource.Cancel(); }))
                    {
                        if (await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)) != task)
                            continue; //request timed out; retry
                    }

                    timeoutCancellationTokenSource.Cancel(); //to stop delay task
                }

                byte[] responseBuffer = await (await task).Content.ReadAsByteArrayAsync();

                stopwatch.Stop();

                //parse response
                using (MemoryStream mS = new MemoryStream(responseBuffer, false))
                {
                    DnsDatagram response = DnsDatagram.ReadFromUdp(mS);

                    response.SetMetadata(new DnsDatagramMetadata(_server, _protocol, responseBuffer.Length, stopwatch.Elapsed.TotalMilliseconds));

                    if (response.Identifier == request.Identifier)
                        return response;
                }
            }

            return null;
        }

        #endregion

        #region properties

        public DateTime LastQueried
        { get { return _lastQueried; } }

        internal bool Pooled
        {
            get { return _pooled; }
            set { _pooled = value; }
        }

        #endregion
    }
}
