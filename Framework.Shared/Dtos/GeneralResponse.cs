namespace Framework.Shared.Dtos
{
    public sealed class GeneralResponse<T>
    {
        public Guid LogId { get; set; }
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
