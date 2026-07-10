namespace ALE.ACS.Domain.Common.Results;

/// <summary>
/// Representa o resultado de uma operação do domínio.
/// </summary>
/// <typeparam name="T">Tipo do valor retornado.</typeparam>
public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Indica se a operação foi executada com sucesso.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Valor retornado pela operação.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Mensagem de erro da operação.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Cria um resultado de sucesso.
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Cria um resultado de falha.
    /// </summary>
    public static Result<T> Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new Result<T>(false, default, error);
    }
}