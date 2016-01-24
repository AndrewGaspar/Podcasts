using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Podcasts.Service
{
    public class BasicAuthStrategy : IAuthStrategy
    {
        #region Private Members

        private string _authToken;

        #endregion Private Members

        public BasicAuthStrategy(string username, string password)
        {
            _authToken = CalculateAuthToken(username, password);
        }

        private static string CalculateAuthToken(string username, string password)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary($"{username}:{password}", BinaryStringEncoding.Utf8);
            return CryptographicBuffer.EncodeToBase64String(buffer);
        }

        public Task AttachCredentials(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new HttpCredentialsHeaderValue("Basic", _authToken);
            return Task.CompletedTask;
        }
    }
}