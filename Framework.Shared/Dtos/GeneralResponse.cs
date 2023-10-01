namespace Framework.Shared.Dtos
{
    public sealed class GeneralResponse<T>
    {
        public Guid LogId { get; set; }
        public T? Data { get; set; }
    }
}
