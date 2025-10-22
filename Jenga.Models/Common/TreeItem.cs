using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Common
{
    public class TreeItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; }
        public List<TreeItem> Children { get; set; } = new List<TreeItem>();
    }
}
