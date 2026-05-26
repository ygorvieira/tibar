using System.Text.Json.Serialization;

namespace Tibar.Application.Common;

public abstract class Result
{
    public bool IsValid { get; protected set; }
    public string[] Errors { get; protected set; } = [];

    public static Result<T> Success<T>(T data) =>
        new() { IsValid = true, Data = data, Errors = [] };

    public static Result<T> Failure<T>(params string[] errors) =>
        new() { IsValid = false, Data = default, Errors = errors };
}

public class Result<T> : Result
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }
}
