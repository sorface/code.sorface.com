namespace Interview.Domain.ServiceResults.Success;

public sealed class CreateServiceResult<T>(T value) : ServiceResult<T>(value)
{
    public override TRes Match<TRes>(
        Func<OkServiceResult<T>, TRes> ok,
        Func<CreateServiceResult<T>, TRes> create,
        Func<NoContentServiceResult<T>, TRes> noContent)
        => create(this);
}
