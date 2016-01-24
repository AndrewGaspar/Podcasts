using System.Threading;
using System.Threading.Tasks;

namespace Podcasts.Service
{
    public interface IAuthStrategy
    {
        Task AttachCredentials(Windows.Web.Http.HttpRequestMessage request, CancellationToken cancellationToken);
    }
}