using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace busfy_api.src.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        User,
        Admin,
    }
}