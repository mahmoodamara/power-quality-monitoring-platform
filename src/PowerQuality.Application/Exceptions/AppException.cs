namespace PowerQuality.Application.Exceptions;
public abstract class AppException : Exception
{
    protected AppException(string code, string message, int statusCode) : base(message) { Code = code; StatusCode = statusCode; }
    public string Code { get; }
    public int StatusCode { get; }
}
public sealed class NotFoundException(string code, string message) : AppException(code, message, 404);
public sealed class ConflictException(string code, string message) : AppException(code, message, 409);
public sealed class ValidationException(string code, string message) : AppException(code, message, 400);
