using System.Net;
using System.Text;

namespace Exchange.Rates.Tests.TestDoubles;

/// <summary>
/// Answers every request with a fixed status code and JSON body, and records the requested URIs.
/// </summary>
public sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string json) : HttpMessageHandler
{
  private readonly List<Uri> _requests = [];

  public IReadOnlyList<Uri> Requests => _requests;

  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    lock (_requests)
    {
      _requests.Add(request.RequestUri!);
    }
    return Task.FromResult(new HttpResponseMessage(statusCode)
    {
      Content = new StringContent(json, Encoding.UTF8, "application/json")
    });
  }
}
