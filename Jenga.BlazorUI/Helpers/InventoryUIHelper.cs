using Jenga.Models.Inventory;
using Jenga.Models.Common;

namespace Jenga.BlazorUI.Helpers
{
    public static class InventoryUIHelper
    {
        /// <summary>
        /// Bir kategorinin tüm üst kategorilerini (Root'a kadar) bulur.
        /// </summary>
        public static List<MaterialCategory> GetCategoryAncestors(int categoryId, List<MaterialCategory>? allCategories)
        {
            var result = new List<MaterialCategory>();
            if (allCategories == null) return result;

            var visited = new HashSet<int>(); // Sonsuz döngü koruması
            var current = allCategories.FirstOrDefault(c => c.Id == categoryId);

            int safety = 0;
            while (current != null && current.ParentCategoryId.HasValue && safety++ < 50)
            {
                var parentId = current.ParentCategoryId.Value;
                if (visited.Contains(parentId)) break;

                var parent = allCategories.FirstOrDefault(c => c.Id == parentId);
                if (parent == null) break;

                result.Insert(0, parent);
                visited.Add(parentId);
                current = parent;
            }
            return result;
        }

        /// <summary>
        /// Bir lokasyonun tam yolunu (Ana Bina > Kat 1 > Oda) döndürür.
        /// </summary>
        public static string GetLocationFullName(int? locationId, List<Location>? allLocations)
        {
            if (allLocations == null || !locationId.HasValue || locationId == 0) return string.Empty;

            var parts = new List<string>();
            var visited = new HashSet<int>();

            var current = allLocations.FirstOrDefault(l => l.Id == locationId.Value);
            int safety = 0;

            while (current != null && safety++ < 50)
            {
                if (visited.Contains(current.Id)) break;
                visited.Add(current.Id);

                parts.Add(current.LocationName);

                if (current.ParentId.HasValue)
                    current = allLocations.FirstOrDefault(l => l.Id == current.ParentId.Value);
                else
                    current = null;
            }

            parts.Reverse();
            return string.Join(" > ", parts);
        }
    }
}