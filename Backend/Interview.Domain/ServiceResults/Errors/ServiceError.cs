namespace Interview.Domain.ServiceResults.Errors;

public class ServiceError(string message) : IEquatable<ServiceError>
{
    public string Message { get; } = message;

    public static ServiceError NotFound(string message) => new NotFoundServiceError(message);

    public static ServiceError Error(string message) => new ServiceError(message);

    public static ServiceError Forbidden(string message) => new ForbiddenError(message);

    public virtual TRes Match<TRes>(
        Func<ServiceError, TRes> appError,
        Func<NotFoundServiceError, TRes> notFoundError,
        Func<ForbiddenError, TRes> forbiddenError)
    {
        return appError(this);
    }

    public void Throw()
    {
        Match<object?>(
            e => throw new UserException(e.Message),
            e => throw new NotFoundException(e.Message),
            e => throw new AccessDeniedException(e.Message));
    }

    public bool Equals(ServiceError? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((ServiceError)obj);
    }

    public override int GetHashCode() => Message.GetHashCode();

    public override string ToString() => Message;
}
