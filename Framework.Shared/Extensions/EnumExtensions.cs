using Framework.Shared.Enums;

namespace Framework.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static PermissionTypes ConvertToEnum(this string[] enumValues)
        {
            PermissionTypes result = PermissionTypes.None;

            foreach (string value in enumValues)
            {
                if (Enum.TryParse(value, out PermissionTypes parsedValue))
                    result |= parsedValue;
                else
                    Console.WriteLine($"Invalid enum value: {value}");
            }

            return result;
        }
    }
}
