using Podcasts.Service.Models;
using Podcasts.Service.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Podcasts.Service
{
    internal class AuthFilter : IHttpFilter
    {
        #region Private Members

        private IAuthStrategy _authStrategy;
        private IHttpFilter _baseFilter;

        #endregion Private Members

        #region Constructors

        public AuthFilter(IAuthStrategy strategy, IHttpFilter baseFilter)
        {
            _authStrategy = strategy;
            _baseFilter = baseFilter;
        }

        public AuthFilter(IAuthStrategy strategy) : this(strategy, new HttpBaseProtocolFilter())
        {
        }

        #endregion Constructors

        IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> IHttpFilter.SendRequestAsync(HttpRequestMessage request) =>
            AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
            {
                await _authStrategy.AttachCredentials(request, cancellationToken);

                return await _baseFilter.SendRequestAsync(request).AsTask(cancellationToken, progress);
            });

        void IDisposable.Dispose()
        {
            _baseFilter.Dispose();
        }
    }

    public class SubscriptionService : IDisposable
    {
        #region Private Members

        private HttpClient _client;
        private IDictionary<Type, DataContractJsonSerializer> _serializers = new Dictionary<Type, DataContractJsonSerializer>();
        private Uri _baseUri;

        #endregion Private Members

        public SubscriptionService(Uri baseUri, IAuthStrategy authStrategy)
        {
            _baseUri = baseUri;
            _client = new HttpClient(new AuthFilter(authStrategy));
            AddNewSerializer<Subscription>(typeof(Podcast));
            AddNewSerializer<SubscriptionRequest>();
        }

        #region Private Helpers

        private void AddNewSerializer<T>(params Type[] knownTypes)
        {
            _serializers[typeof(T)] = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings
            {
                KnownTypes = knownTypes,
                EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.Never,
            });
        }

        private IReadOnlyList<T> DeserializeList<T>(string content) where T : class
        {
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                var result = _serializers[typeof(T)].ReadObject(stream);

                if (result is T) return new[] { result as T };
                else if (result is IReadOnlyList<T>) return result as IReadOnlyList<T>;
                else return new T[] { };
            }
        }

        private string SerializeItem<T>(T item)
        {
            using (var stream = new MemoryStream())
            {
                _serializers[typeof(T)].WriteObject(stream, item);

                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private Uri GetUri(string relativeUri) => new Uri(_baseUri, relativeUri);

        private async Task<string> GetContentAsync(string relativeUri)
        {
            var result = await _client.GetAsync(new Uri(_baseUri, relativeUri));

            return await result.Content.ReadAsStringAsync();
        }

        private async Task<IReadOnlyList<Response>> GetListAsync<Response>(string relativeUri) where Response : class
        {
            return DeserializeList<Response>(await GetContentAsync(relativeUri));
        }

        private async Task<IReadOnlyList<Response>> PostItemAsync<Response, Request>(Request request, string relativeUri) where Response : class
        {
            var response = await _client.PostAsync(
                GetUri(relativeUri),
                new HttpStringContent(
                    SerializeItem(request),
                    UnicodeEncoding.Utf8,
                    "application/json"
                    )
                );

            return DeserializeList<Response>(await response.Content.ReadAsStringAsync());
        }

        #endregion Private Helpers

        public Task<IReadOnlyList<Subscription>> GetSubscriptions() => GetListAsync<Subscription>("subscriptions");

        public async Task<Subscription> PostSubscriptionAsync(string podcastUri)
        {
            return (await PostItemAsync<Subscription, SubscriptionRequest>(new SubscriptionRequest
            {
                Href = podcastUri
            }, "subscriptions")).FirstOrDefault();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}