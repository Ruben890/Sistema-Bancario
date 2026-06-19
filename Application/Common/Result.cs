using System.Net;

namespace Application.Common;

public sealed class Result<TEntity> : IResultResponse
{
    public TEntity? Entity { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public HttpStatusCode StatusCode { get; init; }

    public static Result<TEntity> Response(TEntity entity, string message, HttpStatusCode statusCode)
        => new()
        {
            Entity = entity,
            Message = message,
            StatusCode = statusCode
        };

    public static Result<TEntity> Response(string message, HttpStatusCode statusCode)
        => new()
        {
            Message = message,
            StatusCode = statusCode
        };

    public static Result<TEntity> Failure(string code, string message, HttpStatusCode statusCode)
        => new()
        {
            Code = code,
            Message = message,
            StatusCode = statusCode
        };
}
