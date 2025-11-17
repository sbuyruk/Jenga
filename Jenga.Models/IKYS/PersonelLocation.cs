using Jenga.Models.Inventory;
using Jenga.Models.Sistem;
using System;
using System.Collections.Generic;

namespace Jenga.Models.IKYS
{
    // Join entity: Personel <-> Location
    public class PersonelLocation: BaseModel
    {
        // Composite key: (PersonelId, LocationId) — fluent api ile ayarlayacağız.
        public int PersonelId { get; set; }
        public Personel? Personel { get; set; }

        public int LocationId { get; set; }
        public Location? Location { get; set; }

        // Metadata (opsiyonel ama genelde faydalı)
        public bool IsPrimary { get; set; } = false;
        public string? Role { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

    }
}