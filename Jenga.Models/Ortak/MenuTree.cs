using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Ortak
{
    public class MenuTree
    {
        [Required]
        public int id { get; set; }
        public string? text { get; set; }
        public string? url { get; set; }
        [ValidateNever]
        public string? webpart { get; set; }
        [ValidateNever]
        public int sira { get; set; }
        [ValidateNever]
        public string? aciklama { get; set; }
        [ValidateNever]
        public List<MenuTree>? nodes { get; set; }

    }
}
