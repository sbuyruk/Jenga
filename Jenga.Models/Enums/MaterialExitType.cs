using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Enums
{
    public enum MaterialExitType
    {
        [Display(Name = "Tüketim / Sarf")]
        Consumption = 1,

        [Display(Name = "Satış / Devir")]
        Sale = 2,

        [Display(Name = "Hurda / Zayi / Fire")]
        Scrap = 3,

        [Display(Name = "İade")]
        Return = 4,

        [Display(Name = "Hibe")]
        Donation = 5,

        [Display(Name = "Kayıp")]
        Lost = 6,

        [Display(Name = "Diğer")]
        Other = 99
    }
}