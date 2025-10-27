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
        public T Data { get; set; }
        public List<TreeItem<T>> Children { get; set; } = new();
        public bool ShowCreateIcon { get; set; } = true;
        public bool ShowEditIcon { get; set; } = true;
        public bool ShowDeleteIcon { get; set; } = true;
    }
}
