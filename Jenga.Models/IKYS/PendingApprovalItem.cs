using Jenga.Models.Enums;

namespace Jenga.Models.IKYS;

/// <summary>
/// Represents a single row in the pending approvals list.
/// </summary>
public class PendingApprovalItem
{
    public int GorevOnayId { get; set; }
    public int? PersonelId { get; set; }
    public string? AdiSoyadi { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public string? Sure { get; set; }
    public string? GorevinSebebi { get; set; }
    public string? GorevinYeri { get; set; }
    public string? UlasimAraci { get; set; }
    public string? Transfer { get; set; }
    public string? Konaklama { get; set; }
    public string? Aciklama { get; set; }
    public int AmirOnayi { get; set; }
    public string? OnayRedAciklama { get; set; }

    public ApprovalStatus ApprovalStatus => (ApprovalStatus)AmirOnayi;
}
