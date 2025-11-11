using System.ComponentModel;

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
