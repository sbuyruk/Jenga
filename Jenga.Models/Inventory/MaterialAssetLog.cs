using System;
using System.ComponentModel.DataAnnotations.Schema;
using Jenga.Models.Sistem;

namespace Jenga.Models.Inventory
{
    [Table("MaterialAssetLog_Table")]
    public class MaterialAssetLog : BaseModel
    {
        [Column("MaterialAssetId")]
        public int MaterialAssetId { get; set; }

        [Column("FromPersonelId")]
        public int? FromPersonelId { get; set; }

        [Column("ToPersonelId")]
        public int? ToPersonelId { get; set; }

        [Column("FromLocationId")]
        public int? FromLocationId { get; set; }

        [Column("ToLocationId")]
        public int? ToLocationId { get; set; }

        [Column("TransactionDate")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Column("TransactionType")]
        public string TransactionType { get; set; } = string.Empty;

        public MaterialAsset? MaterialAsset { get; set; }
    }
}
