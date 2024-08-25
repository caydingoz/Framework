using Framework.AuthServer.Enums;
using Framework.Shared.Enums;
using System.Text.Json.Serialization;

namespace Framework.AuthServer.Dtos.RoleService.Input
{
    public class PermissionInput
    {
        public int Id { get; set; } = 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Operations Operation { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PermissionTypes Type { get; set; }
    }
}
