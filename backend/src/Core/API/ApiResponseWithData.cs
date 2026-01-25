namespace Core.API;

public record ApiResponseWithData<T> : Result<T>
    where T : class
{
    public T? Data { get; set; }
}