namespace Zynapse.WebApi.Utils;

public abstract class Result<T>
{
    public abstract TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);
    public abstract void When(Action<T> onSuccess, Action<string> onFailure);

    public static Result<T> Success(T value) => new SuccessResult<T>(value);
    public static Result<T> Failure(string error) => new FailureResult<T>(error);
}

public class SuccessResult<T> : Result<T>
{
    private readonly T _value;

    public SuccessResult(T value)
    {
        _value = value;
    }

    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return onSuccess(_value);
    }

    public override void When(Action<T> onSuccess, Action<string> onFailure)
    {
        onSuccess(_value);
    }
}

public class FailureResult<T> : Result<T>
{
    private readonly string _error;

    public FailureResult(string error)
    {
        _error = error;
    }

    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return onFailure(_error);
    }

    public override void When(Action<T> onSuccess, Action<string> onFailure)
    {
        onFailure(_error);
    }
}