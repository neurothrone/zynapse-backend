using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Shared.Extensions;

public static class ResultExtensions
{
    public static bool IsSuccess<T>(this Result<T> result)
    {
        return result is SuccessResult<T>;
    }

    public static bool IsFailure<T>(this Result<T> result)
    {
        return result is FailureResult<T>;
    }

    public static T Value<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: value => value,
            onFailure: error => throw new InvalidOperationException($"Cannot access Value on a Failure result: {error}")
        );
    }
    
    public static string Error<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: _ => throw new InvalidOperationException("Cannot access Error on a Success result"),
            onFailure: error => error
        );
    }
} 