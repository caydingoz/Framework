using Framework.Shared.Enums;

namespace Framework.Shared.Entities
{
    public class Sort
    {
        public required string Name { get; set; }
        public SortTypes Type { get; set; }
    }
}
