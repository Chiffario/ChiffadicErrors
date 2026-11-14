using System.Diagnostics.CodeAnalysis;
using static ChiffadicErrors.Result;

namespace ChiffadicErrors;

public record ErrorVariant<T>
{
    [SetsRequiredMembers]
    public ErrorVariant(T value) => this.Value = value;
    
    public required T Value { get; init; }
}

public record OkVariant<T> where T : notnull
{
    [SetsRequiredMembers]
    public OkVariant(T value) => this.Value = value;
    
    public required T Value { get; init; }
}

public union Result<TOk, TErr>(OkVariant<TOk>, ErrorVariant<TErr>) where TOk : notnull;

public static class Result
{
    public static OkVariant<TOk> Ok<TOk>(TOk value) where TOk : notnull => new OkVariant<TOk>(value);
    public static ErrorVariant<TErr> Error<TErr>(TErr value) => new ErrorVariant<TErr>(value);
}

public static class ResultExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result) where TOk : notnull
    {
        /// <summary>
        /// Returns <c>true</c> if the result is <c>TOk</c>
        /// </summary>
        /// <returns><c>true</c> if the result is <c>TOk</c></returns>
        public bool IsOk() => result is OkVariant<TOk>;

        public bool IsOkAnd(Predicate<TOk> predicate)
        {
            return result switch
            {
                OkVariant<TOk> val => predicate(val.Value),
                var _   => false
            };
        }

        /// <summary>
        /// Returns <c>true</c> if the result is <c>TErr</c>
        /// </summary>
        /// <returns><c>true</c> if the result is <c>TErr</c></returns>
        public bool IsErr() => result is ErrorVariant<TErr>;

        public bool IsErrAnd(Predicate<TErr> predicate)
        {
            return result switch
            {
                ErrorVariant<TErr> err => predicate(err.Value),
                var _    => false
            };
        }

        public Result<TU, TErr> Bind<TU>(Func<TOk, Result<TU, TErr>> func) where TU : notnull
        {
            return result switch
            {
                OkVariant<TOk> ok      => func(ok.Value),
                ErrorVariant<TErr> err => err,
            };
        }

        public Result<U, TErr> Map<U>(Func<TOk, U> func) where U : notnull
        {
            return result switch
            {
                OkVariant<TOk> ok            => Ok(func(ok.Value)),
                ErrorVariant<TErr> error => error,
            };
        }

        public TOk Expect(string message)
        {
            return result switch
            {
                OkVariant<TOk> ok        => ok.Value,
                ErrorVariant<TErr> error => throw new ResultException(message)
            };
        }

        public TOk Unwrap()
        {
            return result.Expect("called Result::Unwrap() on an Error variant");
        }

        
        public TErr ExpectErr(string message)
        {
            return result switch
            {
                OkVariant<TOk> ok        => throw new ResultException(message),
                ErrorVariant<TErr> error => error.Value
            };
        }

        public TErr UnwrapErr()
        {
            return result.ExpectErr("called Result::UnwrapErr() on an Ok variant");
        }

        public TOk UnwrapOr(TOk defaultValue)
        {
            return result switch
            {
                OkVariant<TOk> ok        => ok.Value,
                ErrorVariant<TErr> error => defaultValue
            };
        }

        public TOk UnwrapOrElse(Func<TErr, TOk> defaultValue)
        {
            return result switch
            {
                OkVariant<TOk> ok        => ok.Value,
                ErrorVariant<TErr> error => defaultValue(error.Value)
            };
        }
    }


    extension<TOk, TErr>(Result<TOk, TErr> result) where TOk : notnull, new()
    {
        public TOk UnwrapOrDefault() 
        {
            return result switch
            {
                OkVariant<TOk> ok        => ok.Value,
                ErrorVariant<TErr> error => new TOk() 
            };
        }     
    }
    
}
