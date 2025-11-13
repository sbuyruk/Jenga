using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Utility.Helpers
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            // Prefer DisplayAttribute.Name (used in many enums), then DescriptionAttribute, then fallback to enum name
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                return displayAttr.Name;

            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                return descAttr.Description;

            return value.ToString();
        }
    }
}
