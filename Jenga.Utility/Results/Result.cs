using System.Diagnostics.CodeAnalysis;

namespace Jenga.Utility.Results;

/// <summary>
/// Servis katmanı için generic olmayan sonuç tipi.
/// Başarılıysa <see cref="IsSuccess"/> = true, aksi halde <see cref="Error"/> doludur.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Başarılı sonuç hata içeremez.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Başarısız sonucun hata bilgisi olmalı.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Generic sonuç tipi: başarılıysa <see cref="Value"/> doludur.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Sadece <see cref="Result.IsSuccess"/> = true iken erişilmelidir.
    /// </summary>
    [NotNull]
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Failure sonucundan Value okunamaz. Önce IsSuccess kontrol edin.");

    public new static Result<T> Success(T value) => new(true, value, Error.None);
    public new static Result<T> Failure(Error error) => new(false, default, error);

    // Implicit dönüşüm: dönüş satırlarında "return value;" yazılabilir.
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
