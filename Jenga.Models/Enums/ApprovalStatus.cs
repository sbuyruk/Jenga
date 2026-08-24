using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Enums;

public enum ApprovalStatus
{
    [Display(Name = "Onay Bekliyor")]
    PendingApproval = 0,

    [Display(Name = "Onaylandı")]
    Approved = 1,

    [Display(Name = "Reddedildi")]
    Rejected = 2,

    [Display(Name = "Onay Gerekmiyor")]
    NotRequired = 3,

    [Display(Name = "Diğer")]
    Other = 4
}
