using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Enums
{
    public enum LocationType
    {
        [Description("Bina")]
        Building,
        [Description("Kat")]
        Floor,
        [Description("Depo")]
        Warehouse,
        [Description("Oda")]
        Room,
        [Description("Dolap")]
        Cabinet,
        [Description("Raf")]
        Shelf,
        [Description("Depolama Alanı")]
        StoragePlace // Malzeme konulabilen son nokta
    }
}
