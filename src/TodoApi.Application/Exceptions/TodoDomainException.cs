namespace TodoApi.Application.Exceptions;

public sealed class TodoDomainException : Exception
{
    public string Title { get; }
    public string Detail { get; }
    public int StatusCode { get; }

    public TodoDomainException(string title, string detail, int statusCode)
        : base(detail)
    {
        Title = title;
        Detail = detail;
        StatusCode = statusCode;
    }
}
