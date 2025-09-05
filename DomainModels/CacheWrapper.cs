namespace DomainModels;

public class CacheWrapper<T>
{
    public T Data { get; set; } = default!;
    public DateTime Expiry { get; set; }
}