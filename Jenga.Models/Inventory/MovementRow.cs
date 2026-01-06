using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Inventory
{
    public class MovementRow
    {
        public int Id { get; set; }

        // Display strings
        public string ActionType { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string QuantityText { get; set; } = string.Empty;
        public string FromLocation { get; set; } = string.Empty;
        public string FromPerson { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public string ToPerson { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;

        // New: typed properties used for correct sorting
        public DateTime DateValue { get; set; }
        public decimal QuantityValue { get; set; }
    }
}
