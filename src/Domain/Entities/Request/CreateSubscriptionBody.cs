using System.ComponentModel.DataAnnotations;
using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Request
{
    public class CreateSubscriptionBody
    {
        [Range(1, float.MaxValue)]
        public float Price { get; set; }

        [EnumDataType(typeof(SubscriptionType))]
        public SubscriptionType Type { get; set; }

        [Range(1, int.MaxValue)]
        public int CountDays { get; set; }
    }
}