using Jenga.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenga.Models.Helper
{
    public static class TreeHelper
    {
        /// <summary>
        /// Düz listeyi TreeItem<T> generic hiyerarşisine dönüştürür.
        /// </summary>
        public static List<TreeItem<T>> BuildGenericTree<T, TKey>(
            List<T> flatList,
            Func<T, TKey> idSelector,
            Func<T, TKey?> parentIdSelector,
            Func<T, TreeItem<T>> itemFactory)
            where TKey : struct
        {
            var lookup = flatList.ToDictionary(idSelector, itemFactory);
            var roots = new List<TreeItem<T>>();

            foreach (var item in flatList)
            {
                var node = lookup[idSelector(item)];
                var parentId = parentIdSelector(item);

                // Eğer ParentId null veya default ise, kök
                if (!parentId.HasValue || parentId.Value.Equals(default(TKey)))
                {
                    roots.Add(node);
                }
                else if (lookup.ContainsKey(parentId.Value))
                {
                    lookup[parentId.Value].Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }
            return roots;
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
    }


}