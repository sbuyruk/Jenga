using Jenga.Models.Common;

namespace Jenga.Models.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItem> BuildTree(List<MenuItem> flatList)
        {
            if (flatList == null || flatList.Count == 0)
                return new List<MenuItem>();

            var distinct = flatList
                .Where(item => item != null)
                .GroupBy(item => item.Id)
                .Select(g =>
                {
                    if (g.Count() > 1)
                        Console.Error.WriteLine($"MenuHelper.BuildTree: duplicate MenuItem.Id {g.Key} found ({g.Count()} entries). Using first occurrence.");
                    return g.First();
                })
                .ToList();

            var lookup = distinct.ToDictionary(item => item.Id);
            var rootItems = new List<MenuItem>();

            foreach (var item in distinct)
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

            foreach (var item in distinct)
            {
                if (item.Children?.Any() == true)
                    item.Children = item.Children.OrderBy(c => c.DisplayOrder).ToList();
            }

            return rootItems.OrderBy(i => i.DisplayOrder).ToList();
        }
    }
}
