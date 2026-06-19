using System.Net;

namespace Application.Common;

public interface IResultResponse
{
    HttpStatusCode StatusCode { get; }
}
