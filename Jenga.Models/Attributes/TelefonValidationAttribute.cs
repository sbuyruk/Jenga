using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Attributes
{
    /**
     * SB Telefon numarasi dogrulama için özel bir dogrulama sinifi 
     * 
     * örnek kullanimi
     public class TasinmazBagisci : BaseModel
        {
            [DisplayName("Telefon 1")]
            [TelefonValidation]
            public string? Telefon1 { get; set; }
            [DisplayName("Telefon 2")]
            [TelefonValidation]
            public string? Telefon2 { get; set; }
        }
     *
     *Regex Açiklamasi:
        ^ ve $: Ifadenin baslangicini ve sonunu temsil eder.
        \+90: Telefon numarasinin +90 ile baslamasini saglar.
        \d{10}: 10 basamakli rakamlardan olusmasini bekler.
     **/

    public class TelefonValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string telefon && !string.IsNullOrEmpty(telefon))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(telefon, @"^\+90\d{10}$"))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult("Telefon numarasi '+90XXXXXXXXXX' formatinda olmalidir.");
            }
            return ValidationResult.Success; // Bossa geçerli kabul edilir.
        }
    }
}
