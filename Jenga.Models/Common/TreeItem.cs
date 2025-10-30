using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Common
{
    /// <summary>
    /// Ağaç düğümü generic modeli.
    /// </summary>
    public class TreeItem<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public T Data { get; set; } = default!;
        public List<TreeItem<T>> Children { get; set; } = new();
        public bool ShowContextMenu { get; set; } = true;
        public bool ShowCreate { get; set; } = true;
        public bool ShowEdit { get; set; } = true;
        public bool ShowDelete { get; set; } = true;
    }
}
