using System.Net.Http;

namespace Conectify.Shared.Library.Services;

public interface IHttpFactory
{
    public HttpClient HttpClient { get; }
}

public class HttpFactory : IHttpFactory
{
    public HttpClient HttpClient => new();
}
