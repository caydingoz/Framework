namespace Framework.Domain.Interfaces.Entities
{
    public interface ICachable
    {
        string GetCacheKey() => GetType().FullName;
        TimeSpan? GetExpireTime() => new(TimeSpan.TicksPerDay * 365);
    }
}
