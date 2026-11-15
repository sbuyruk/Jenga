using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using System.Globalization;

namespace Jenga.BlazorUI.Components.IKYS;

public static class PendingApprovalExport
{
    public static IReadOnlyList<string> TaskHeaders { get; } =
    [
        "Adı Soyadı", "Başlangıç Tarihi", "Bitiş Tarihi", "Görevin Sebebi", "Görevin Yeri",
        "Ulaşım Aracı", "Transfer", "Konaklama", "Açıklama"
    ];

    public static IReadOnlyList<string> ReviewHeaders { get; } = [.. TaskHeaders, "Durum", "Ret Gerekçesi"];

    public static IReadOnlyList<object?> TaskRow(PendingApprovalItem item) => CreateTaskRow(item, "dd.MM.yyyy HH:mm");

    public static IReadOnlyList<object?> ReviewRow(PendingApprovalItem item) =>
    [
        .. CreateTaskRow(item, "dd.MM.yyyy"),
        item.ApprovalStatus switch
        {
            ApprovalStatus.Approved => "Onaylandı",
            ApprovalStatus.Rejected => "Reddedildi",
            ApprovalStatus.NotRequired => "Onay Gerekmiyor",
            _ => "Onay Bekliyor"
        },
        item.ApprovalStatus == ApprovalStatus.Rejected ? item.OnayRedAciklama : null
    ];

    private static IReadOnlyList<object?> CreateTaskRow(PendingApprovalItem item, string dateFormat) =>
    [
        item.AdiSoyadi,
        item.BaslangicTarihi?.ToString(dateFormat, CultureInfo.InvariantCulture),
        item.BitisTarihi?.ToString(dateFormat, CultureInfo.InvariantCulture),
        item.GorevinSebebi, item.GorevinYeri, item.UlasimAraci,
        item.Transfer, item.Konaklama, item.Aciklama
    ];
}
