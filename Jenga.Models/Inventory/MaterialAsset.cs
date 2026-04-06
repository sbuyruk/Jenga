using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jenga.Models.IKYS;
using Jenga.Models.Sistem;

namespace Jenga.Models.Inventory
{
    [Table("MaterialAsset_Table")]
    public class MaterialAsset : BaseModel
    {
        [Column("MaterialId")]
        public int MaterialId { get; set; }

        [Column("BrandId")]
        public int? BrandId { get; set; }

        [Column("ModelId")]
        public int? ModelId { get; set; }

        [Column("SerialNumber")]
        public string? SerialNumber { get; set; }

        [Column("Barcode")]
        public string? Barcode { get; set; }

        [Column("PurchaseDate")]
        public DateTime? PurchaseDate { get; set; }

        [Column("WarrantyExpiry")]
        public DateTime? WarrantyExpiry { get; set; }

        [Column("Status")]
        public AssetStatus Status { get; set; } = AssetStatus.Active;

        [Column("LocationId")]
        public int? LocationId { get; set; }

        [Column("PersonelId")]
        public int? PersonelId { get; set; }

        public Material? Material { get; set; }
        public MaterialBrand? Brand { get; set; }
        public MaterialModel? Model { get; set; }
        public Location? Location { get; set; }
        public Personel? Personel { get; set; }
    }

    public enum AssetStatus
    {
        Active = 1,
        InRepair = 2,
        Retired = 3,
        Lost = 4
    }
}
