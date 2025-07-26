
using Jenga.Models.Common;

namespace Jenga.Utility.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItem> BuildTree(List<MenuItem> flatList)
        {
            var lookup = flatList.ToDictionary(item => item.Id);
            var rootItems = new List<MenuItem>();

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.ContainsKey(item.ParentId.Value))
                {
                    var parent = lookup[item.ParentId.Value];
                    parent.Children ??= new List<MenuItem>();
                    parent.Children.Add(item);
                }
                else
                {
                    rootItems.Add(item);
                }
            }

            // Opsiyonel: DisplayOrder'a göre sıralama
            foreach (var item in flatList)
            {
                if (item.Children?.Any() == true)
                    item.Children = item.Children.OrderBy(c => c.DisplayOrder).ToList();
            }

            return rootItems.OrderBy(i => i.DisplayOrder).ToList();
        }
    }

}
