using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jenga.Models.Helpers
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
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
