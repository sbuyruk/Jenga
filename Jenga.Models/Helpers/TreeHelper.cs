using Jenga.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenga.Models.Helper
{
    public static class TreeHelper
    {
        public static List<TreeItem<T>> BuildTree<T>(
        IEnumerable<T> items,
        Func<T, string> idSelector,
        Func<T, string?> parentIdSelector,
        Func<T, List<TreeItem<T>>>? extraChildrenSelector = null)
        {
            var lookup = items.ToDictionary(idSelector, item =>
                new TreeItem<T>
                {
                    Id = idSelector(item),
                    Data = item,
                    Children = new List<TreeItem<T>>()
                });

            foreach (var item in items)
            {
                var parentId = parentIdSelector(item);
                if (parentId != null && lookup.ContainsKey(parentId))
                    lookup[parentId].Children.Add(lookup[idSelector(item)]);
            }

            // Add extra children if needed (e.g., materials under categories)
            if (extraChildrenSelector != null)
            {
                foreach (var item in items)
                {
                    var children = extraChildrenSelector(item);
                    if (children != null && children.Count > 0)
                        lookup[idSelector(item)].Children.AddRange(children);
                }
            }

            // Return root nodes (no parent)
            return lookup.Values.Where(n => parentIdSelector(n.Data) == null).ToList();
        }
        

        /// <summary>
        /// Generic tree için filtreleme, klonlama ile yeni ağaç döndürür.
        /// </summary>
        public static List<TreeItem<T>> FilterTree<T>(
            List<TreeItem<T>> nodes,
            Func<TreeItem<T>, bool> predicate)
        {
            var filtered = new List<TreeItem<T>>();
            foreach (var node in nodes)
            {
                var match = FilterNode(node, predicate);
                if (match != null)
                    filtered.Add(match);
            }
            return filtered;
        }

        private static TreeItem<T>? FilterNode<T>(
            TreeItem<T> node,
            Func<TreeItem<T>, bool> predicate)
        {
            bool matches = predicate(node);
            var matchingChildren = new List<TreeItem<T>>();

            foreach (var child in node.Children)
            {
                var matchedChild = FilterNode(child, predicate);
                if (matchedChild != null)
                    matchingChildren.Add(matchedChild);
            }

            if (matchingChildren.Count > 0)
                matches = true;

            if (matches)
            {
                // Klonla ve çocukları ekle
                return new TreeItem<T>
                {
                    Data = node.Data,
                    Children = matchingChildren,
                    ShowCreate = node.ShowCreate,
                    ShowEdit = node.ShowEdit,
                    ShowDelete = node.ShowDelete
                };
            }
            return null;
        }
        //Expand Collapse
        /// <summary>
        /// Collects the expanded states of all nodes in a tree structure and returns them as a dictionary.
        /// </summary>
        /// <remarks>This method recursively traverses the tree structure, including all child nodes, to
        /// collect the expanded states.</remarks>
        /// <typeparam name="T">The type of the data contained in the tree nodes.</typeparam>
        /// <param name="nodes">A list of tree nodes to process. Each node may have child nodes.</param>
        /// <returns>A dictionary where the keys are the unique identifiers of the nodes and the values indicate whether the
        /// nodes are expanded.</returns>
        public static Dictionary<string, bool> CollectExpandedStates<T>(List<TreeItem<T>> nodes)
        {
            var dict = new Dictionary<string, bool>();
            foreach (var node in nodes)
            {
                dict[node.Id] = node.IsExpanded;
                if (node.Children != null && node.Children.Count > 0)
                {
                    foreach (var kvp in CollectExpandedStates(node.Children))
                        dict[kvp.Key] = kvp.Value;
                }
            }
            return dict;
        }

        public static void RestoreExpandedStates<T>(List<TreeItem<T>> nodes, Dictionary<string, bool> expandedMap)
        {
            foreach (var node in nodes)
            {
                if (expandedMap.TryGetValue(node.Id, out var isExpanded))
                    node.IsExpanded = isExpanded;
                if (node.Children != null && node.Children.Count > 0)
                    RestoreExpandedStates(node.Children, expandedMap);
            }
        }
    }


}