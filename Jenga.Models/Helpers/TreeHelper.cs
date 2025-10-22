using Jenga.Models.Common;
using Jenga.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Helper
{
    public static class TreeHelper
    {
        public static List<T> BuildTree<T, TKey>(
            List<T> flatList,
            Func<T, TKey> idSelector,
            Func<T, TKey?> parentIdSelector,
            Action<T, List<T>> setChildren)
            where TKey : struct
        {
            var lookup = flatList.ToLookup(parentIdSelector);
            foreach (var item in flatList)
            {
                var children = lookup[idSelector(item)].ToList();
                setChildren(item, children);
            }
            return lookup[null].ToList(); // ParentId'si null olanlar kök düğümler
        }
        public static List<Location> FilterTree(List<Location> nodes, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return nodes;
            var filtered = new List<Location>();

            foreach (var node in nodes)
            {
                // Düğüm veya childlarında eşleşme varsa ekle
                if (NodeMatches(node, searchTerm, out var matchedNode))
                    filtered.Add(matchedNode);
            }
            return filtered;
        }

        private static bool NodeMatches(Location node, string searchTerm, out Location matched)
        {
            bool matches = node.LocationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            var matchingChildren = new List<Location>();

            foreach (var child in node.Children)
            {
                if (NodeMatches(child, searchTerm, out var matchedChild))
                    matchingChildren.Add(matchedChild);
            }

            if (matchingChildren.Count > 0)
                matches = true;

            if (matches)
            {
                matched = new Location
                {
                    Id = node.Id,
                    LocationName = node.LocationName,
                    LocationType = node.LocationType,
                    ParentId = node.ParentId,
                    Aciklama = node.Aciklama,
                    IsStoragePlace = node.IsStoragePlace,
                    Children = matchingChildren
                };
                return true;
            }

            matched = null;
            return false;
        }
    }

}
