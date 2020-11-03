using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace Gelf.Extensions.Logging
{
    public class HttpGelfClient : IGelfClient
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler? _httpClientHandler;
        
        public HttpGelfClient(GelfLoggerOptions options)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = options.Protocol.ToString().ToLower(),
                Host = options.Host,
                Port = options.Port
            };

            _httpClientHandler = new HttpClientHandler();
            if (options.HttpSslProtocols != null)
            {
                _httpClientHandler.SslProtocols = (SslProtocols)options.HttpSslProtocols;
            }
            if (options.HttpCertificates != null)
            {
                _httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                _httpClientHandler.ClientCertificates.AddRange(options.HttpCertificates);
            }
            if (options.HttpCheckCertificateRevocation)
            {
                _httpClientHandler.CheckCertificateRevocationList = options.HttpCheckCertificateRevocation;
            }
            if (options.HttpServerCertificateValidator != null)
            {
                _httpClientHandler.ServerCertificateCustomValidationCallback = options.HttpServerCertificateValidator;
            }
            _httpClient = new HttpClient(_httpClientHandler, true);
            _httpClient.BaseAddress = uriBuilder.Uri;
            _httpClient.Timeout = options.HttpTimeout;

            if (options.HttpHeaders != null)
            {
                foreach (var header in options.HttpHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var content = new StringContent(message.ToJson(), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("gelf", content);
            result.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
