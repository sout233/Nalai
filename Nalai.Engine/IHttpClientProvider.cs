using System.Net.Http;

namespace Nalai.Engine;

public interface IHttpClientProvider
{
    HttpClient GetClient();
}